using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CenterManagement.Application.DTOs.Attendance;
using CenterManagement.Application.DTOs.Common;
using CenterManagement.Application.DTOs.Student;
using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;

namespace CenterManagement.Application.Services;  

public class AttendanceService : IAttendanceService
{
    private readonly CenterManagementDbContext _db;
    private readonly IQrService _qrService;
    private readonly IAuditLogService _auditLogService;
    private readonly IConfiguration _config;
    private readonly INotificationService _notificationService;

    public AttendanceService(
        CenterManagementDbContext db,
        IQrService qrService,
        IAuditLogService auditLogService,
        IConfiguration config,
        INotificationService notificationService)
    {
        _db = db;
        _qrService = qrService;
        _auditLogService = auditLogService;
        _config = config;
        _notificationService = notificationService;
    }

    public async Task<Session?> GetSessionForStudentAtTimeAsync(int studentProfileId, DateTime scanTime)
    {
        var scanTimeOfDay = scanTime.TimeOfDay;
        var scanDate = scanTime.Date;
        var graceMinutes = 30;
        var earlyGraceMinutes = 30; // 30 minutes before start
        var adjustedScanTimeForEnd = scanTimeOfDay.Subtract(TimeSpan.FromMinutes(graceMinutes));
        var adjustedScanTimeForStart = scanTimeOfDay.Add(TimeSpan.FromMinutes(earlyGraceMinutes));

        return await _db.Enrollments
            .Where(e =>
                e.StudentProfileId == studentProfileId &&
                e.IsActive &&
                !e.IsDeleted)
            .SelectMany(e => e.Group.Sessions)
            .Where(s =>
                !s.IsDeleted &&
                !s.IsCanceled &&
                s.SessionDate.Date == scanDate &&
                s.StartTime <= adjustedScanTimeForStart &&
                s.EndTime >= adjustedScanTimeForEnd)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<Session?> GetSessionForInstructorAtTimeAsync(int instructorProfileId, DateTime scanTime)
    {
        var scanTimeOfDay = scanTime.TimeOfDay;
        var scanDate = scanTime.Date;
        var graceMinutes = 30;
        var earlyGraceMinutes = 30; // 30 minutes before start
        var adjustedScanTimeForEnd = scanTimeOfDay.Subtract(TimeSpan.FromMinutes(graceMinutes));
        var adjustedScanTimeForStart = scanTimeOfDay.Add(TimeSpan.FromMinutes(earlyGraceMinutes));

        return await _db.Sessions
            .Include(s => s.Group)
            .Where(s =>
                (s.Group.InstructorProfileId == instructorProfileId || s.SubstituteInstructorProfileId == instructorProfileId) &&
                !s.IsDeleted &&
                !s.IsCanceled &&
                s.SessionDate.Date == scanDate &&
                s.StartTime <= adjustedScanTimeForStart &&
                s.EndTime >= adjustedScanTimeForEnd)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<ScanResultDto> ProcessScanAsync(string qrCode, DateTime scanTime)
    {
        try
        {
            var userId = _qrService.DecodeQrCode(qrCode);
            if (string.IsNullOrEmpty(userId))
            {
                // Cannot log to QrCodeLog because 'INVALID' is not a valid ApplicationUser FK
                return new ScanResultDto { Success = false, ErrorMessage = "Invalid QR code format." };
            }

            var studentProfile = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
            var instructorProfile = await _db.InstructorProfiles.FirstOrDefaultAsync(i => i.UserId == userId && !i.IsDeleted);

            if (studentProfile == null && instructorProfile == null)
            {
                await WriteQrCodeLogAsync(qrCode, scanTime, userId);
                return new ScanResultDto { Success = false, ErrorMessage = "User not found" };
            }

            Session? session = null;
            bool isLate = false;
            int? attendanceId = null;

            if (studentProfile != null)
            {
                session = await GetSessionForStudentAtTimeAsync(studentProfile.Id, scanTime);
                if (session == null)
                {
                    await WriteQrCodeLogAsync(qrCode, scanTime, userId);
                    return new ScanResultDto { Success = false, ErrorMessage = "No active session found at this time" };
                }

                // GAP-011: 50% Late Rejection
                var sessionDuration = session.EndTime - session.StartTime;
                var halfwayPoint = session.StartTime.Add(TimeSpan.FromTicks(sessionDuration.Ticks / 2));
                var isRejected = scanTime.TimeOfDay > halfwayPoint;
                if (isRejected)
                {
                    await WriteQrCodeLogAsync(qrCode, scanTime, userId);
                    return new ScanResultDto 
                    { 
                        Success = false, 
                        ErrorMessage = "Scan rejected: more than 50% of the session has elapsed."
                    };
                }

                var existingAttendance = await _db.StudentAttendances
                    .FirstOrDefaultAsync(a => a.StudentProfileId == studentProfile.Id && a.SessionId == session.Id);

                if (existingAttendance != null)
                {
                    // GAP-010: Duplicate Scan Tracking
                    existingAttendance.DuplicateScanCount++;
                    await _db.SaveChangesAsync(); // Save count

                    await WriteQrCodeLogAsync(qrCode, scanTime, userId);

                    if (existingAttendance.DuplicateScanCount >= 3)
                    {
                        return new ScanResultDto { Success = false, ErrorMessage = "Warning: Multiple scan attempts detected." };
                    }
                    
                    return new ScanResultDto { Success = true, ErrorMessage = "Already scanned", IsLate = existingAttendance.IsLate };
                }

                var graceMinutes = _config.GetValue<int>("AppSettings:AttendanceLateGraceMinutes", 15);
                isLate = scanTime.TimeOfDay > session.StartTime.Add(TimeSpan.FromMinutes(graceMinutes));

                var attendance = new StudentAttendance
                {
                    StudentProfileId = studentProfile.Id,
                    SessionId = session.Id,
                    IsPresent = true,
                    IsLate = isLate,
                    IsRejected = false,
                    ScanTime = scanTime
                };

                _db.StudentAttendances.Add(attendance);

                try
                {
                    await _db.SaveChangesAsync();
                    attendanceId = attendance.Id;

                    // GAP-003: Unpaid Student Notification
                    var unpaidPayment = await _db.StudentCoursePayments
                        .Where(p => p.StudentProfileId == studentProfile.Id && p.CourseId == session.Group.CourseId && !p.IsPaid && !p.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (unpaidPayment != null)
                    {
                        var admins = await _db.UserRoles
                            .Where(ur => ur.RoleId == _db.Roles.FirstOrDefault(r => r.Name == "Admin")!.Id)
                            .Select(ur => ur.UserId)
                            .ToListAsync();

                        foreach (var aId in admins)
                        {
                            await _notificationService.SendToUserAsync(aId, "Unpaid Student Attended", 
                                $"Student {studentProfile.User.FullName} attended session {session.Id} but has an outstanding balance of {unpaidPayment.RemainingAmount:C}.");
                        }
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true || ex.InnerException?.Message.Contains("duplicate") == true)
                {
                    return new ScanResultDto { Success = true, ErrorMessage = "Already scanned" };
                }
            }
            else if (instructorProfile != null)
            {
                session = await GetSessionForInstructorAtTimeAsync(instructorProfile.Id, scanTime);
                if (session == null)
                {
                    await WriteQrCodeLogAsync(qrCode, scanTime, userId);
                    return new ScanResultDto { Success = false, ErrorMessage = "No active session found at this time" };
                }

                var existingAttendance = await _db.InstructorAttendances
                    .FirstOrDefaultAsync(a => a.InstructorProfileId == instructorProfile.Id && a.SessionId == session.Id);

                if (existingAttendance != null)
                {
                    await WriteQrCodeLogAsync(qrCode, scanTime, userId);
                    return new ScanResultDto { Success = true, ErrorMessage = "Already scanned", IsLate = existingAttendance.IsLate };
                }

                var graceMinutes = _config.GetValue<int>("AppSettings:AttendanceLateGraceMinutes", 15);
                isLate = scanTime.TimeOfDay > session.StartTime.Add(TimeSpan.FromMinutes(graceMinutes));

                var attendance = new InstructorAttendance
                {
                    InstructorProfileId = instructorProfile.Id,
                    SessionId = session.Id,
                    IsPresent = true,
                    IsLate = isLate,
                    ScanTime = scanTime
                };

                _db.InstructorAttendances.Add(attendance);

                try
                {
                    await _db.SaveChangesAsync();
                    attendanceId = attendance.Id;
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true || ex.InnerException?.Message.Contains("duplicate") == true)
                {
                    return new ScanResultDto { Success = true, ErrorMessage = "Already scanned" };
                }
            }

            // Load session with relations for response
            session = await _db.Sessions
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.GradeLevel)
                .Include(s => s.Group).ThenInclude(g => g.InstructorProfile).ThenInclude(i => i.User)
                .FirstOrDefaultAsync(s => s.Id == session!.Id);

            await WriteQrCodeLogAsync(qrCode, scanTime, userId);

            // Fetch StudentUser if student
            string? studentName = null;
            string? studentImagePath = null;
            if (studentProfile != null)
            {
                var studentUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                studentName = studentUser?.FullName;
                studentImagePath = studentUser?.ImagePath;
            }

            // GAP-003: Unpaid Student Notification (Frontend Data)
            bool hasUnpaid = false;
            decimal unpaidAmount = 0m;
            
            if (studentProfile != null && session?.Group?.CourseId != null)
            {
                var unpaidPayment = await _db.StudentCoursePayments
                    .Where(p => p.StudentProfileId == studentProfile.Id && p.CourseId == session.Group.CourseId && !p.IsPaid && !p.IsDeleted)
                    .FirstOrDefaultAsync();
                    
                if (unpaidPayment != null)
                {
                    hasUnpaid = true;
                    unpaidAmount = unpaidPayment.RemainingAmount;
                }
            }

            return new ScanResultDto
            {
                Success = true,
                StudentName = studentName,
                StudentImagePath = studentImagePath,
                SessionTitle = session?.Group?.Course?.Name,
                GroupName = session?.Group?.Name,
                InstructorName = session?.Group?.InstructorProfile?.User?.FullName,
                GradeLevelName = session?.Group?.Course?.GradeLevel?.Name,
                IsLate = isLate,
                ScanTime = scanTime,
                AttendanceId = attendanceId,
                HasUnpaidBalance = hasUnpaid,
                UnpaidAmount = unpaidAmount
            };
        }
        catch (Exception ex)
        {
            await _auditLogService.LogAsync("SYSTEM", "ProcessScanError", "System", 0, null, ex.Message);
            return new ScanResultDto { Success = false, ErrorMessage = "Scan failed: " + ex.Message };
        }
    }

    private async Task WriteQrCodeLogAsync(string qrCode, DateTime scanTime, string userId)
    {
        var log = new QrCodeLog
        {
            QrCode = qrCode,
            ScanTime = scanTime,
            UserId = userId
        };
        _db.QrCodeLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<AttendanceSessionSummaryDto> GetSessionSummaryAsync(int sessionId)
    {
        var session = await _db.Sessions
            .Include(s => s.Group).ThenInclude(g => g.Enrollments)
            .Include(s => s.Attendances)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return new AttendanceSessionSummaryDto();

        var totalEnrolled = session.Group.Enrollments.Count(e => e.IsActive && !e.IsDeleted);
        var present = session.Attendances.Count(a => a.IsPresent && !a.IsDeleted);
        var late = session.Attendances.Count(a => a.IsLate && !a.IsDeleted);
        var absent = totalEnrolled - present;
        var rate = totalEnrolled > 0 ? (decimal)present / totalEnrolled * 100 : 0;

        return new AttendanceSessionSummaryDto
        {
            SessionId = sessionId,
            TotalEnrolled = totalEnrolled,
            PresentCount = present,
            AbsentCount = absent,
            LateCount = late,
            AttendanceRatePercent = rate
        };
    }

    public async Task MarkManuallyAsync(ManualMarkDto dto, string adminId)
    {
        var attendance = await _db.StudentAttendances
            .FirstOrDefaultAsync(a => a.StudentProfileId == dto.StudentProfileId && a.SessionId == dto.SessionId && !a.IsDeleted);

        if (attendance != null)
        {
            attendance.IsPresent = dto.IsPresent;
            attendance.IsLate = dto.IsLate;
            attendance.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            attendance = new StudentAttendance
            {
                StudentProfileId = dto.StudentProfileId,
                SessionId = dto.SessionId,
                IsPresent = dto.IsPresent,
                IsLate = dto.IsLate,
                ScanTime = DateTime.UtcNow
            };
            _db.StudentAttendances.Add(attendance);
        }

        await _db.SaveChangesAsync();
        await _auditLogService.LogAsync(adminId, "ManualAttendanceMark", "StudentAttendance", dto.StudentProfileId, null, $"Session: {dto.SessionId}");
    }

    public async Task<PagedResult<AttendanceListItemDto>> GetSessionAttendanceListAsync(int sessionId, int page, int pageSize, string? search)
    {
        var query = _db.StudentAttendances
            .IgnoreQueryFilters()
            .Include(a => a.StudentProfile).ThenInclude(sp => sp.User)
            .Where(a => a.SessionId == sessionId && !a.IsDeleted);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => a.StudentProfile.User.FullName.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.StudentProfile.User.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AttendanceListItemDto
            {
                AttendanceId = a.Id,
                StudentProfileId = a.StudentProfileId,
                StudentName = a.StudentProfile.User.FullName,
                StudentImagePath = a.StudentProfile.User.ImagePath,
                IsPresent = a.IsPresent,
                IsLate = a.IsLate,
                ScanTime = a.ScanTime
            })
            .ToListAsync();

        return new PagedResult<AttendanceListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<StudentAttendanceDto>> GetStudentAttendanceHistoryAsync(int studentProfileId, int? groupId, DateTime? from, DateTime? to)
    {
        var query = _db.StudentAttendances
            .Include(a => a.Session).ThenInclude(s => s.Group).ThenInclude(g => g.Course)
            .Where(a => a.StudentProfileId == studentProfileId && !a.IsDeleted);

        if (groupId.HasValue)
        {
            query = query.Where(a => a.Session.GroupId == groupId.Value);
        }
        if (from.HasValue)
        {
            query = query.Where(a => a.ScanTime >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(a => a.ScanTime <= to.Value);
        }

        return await query
            .OrderByDescending(a => a.ScanTime)
            .Select(a => new StudentAttendanceDto
            {
                AttendanceId = a.Id,
                SessionDate = a.Session.SessionDate,
                GroupName = a.Session.Group.Name,
                SubjectName = a.Session.Group.Course.Subject.Name,
                IsPresent = a.IsPresent,
                IsLate = a.IsLate,
                ScanTime = a.ScanTime
            })
            .ToListAsync();
    }

    public async Task<decimal> GetStudentAttendanceRateAsync(int studentProfileId)
    {
        var total = await _db.StudentAttendances
            .CountAsync(a => a.StudentProfileId == studentProfileId && !a.IsDeleted);
        if (total == 0) return 0;
        var present = await _db.StudentAttendances
            .CountAsync(a => a.StudentProfileId == studentProfileId && a.IsPresent && !a.IsDeleted);
        return (decimal)present / total * 100;
    }
}
