using CenterManagement.Application.DTOs.Attendance;
using CenterManagement.Application.DTOs.Common;
using CenterManagement.Application.DTOs.Student;
using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CenterManagement.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly CenterManagementDbContext _db;
        private readonly IQrService _qrService;
        private readonly IAuditLogService _auditLogService;
        private readonly IConfiguration _config;

        public AttendanceService(
            CenterManagementDbContext db,
            IQrService qrService,
            IAuditLogService auditLogService,
            IConfiguration config)
        {
            _db = db;
            _qrService = qrService;
            _auditLogService = auditLogService;
            _config = config;
        }

        // =========================================
        // Session Resolution — Server-side EF Core
        // =========================================

        public async Task<Session?> GetSessionForStudentAtTimeAsync(int studentProfileId, DateTime scanTime)
        {
            var scanTimeOfDay = scanTime.TimeOfDay;
            var scanDate = scanTime.Date;
            var graceWindow = TimeSpan.FromMinutes(30); // scan-out grace

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
                    s.StartTime <= scanTimeOfDay &&
                    s.EndTime.Add(graceWindow) >= scanTimeOfDay)
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefaultAsync();
        }

        // =========================================
        // ProcessScanAsync — 9-step flow
        // =========================================

        public async Task<ScanResultDto> ProcessScanAsync(string qrCode, DateTime scanTime)
        {
            try
            {
                // Step 1: Decode QR → userId
                var userId = _qrService.DecodeQrCode(qrCode);
                if (userId == null)
                {
                    _db.QrCodeLogs.Add(new QrCodeLog
                    {
                        QrCode = qrCode,
                        ScanTime = scanTime,
                        UserId = "INVALID"
                    });
                    await _db.SaveChangesAsync();
                    return new ScanResultDto { Success = false, ErrorMessage = "Invalid QR code" };
                }

                // Step 2: Find StudentProfile by UserId
                var studentProfile = await _db.StudentProfiles
                    .Include(sp => sp.User)
                    .Include(sp => sp.GradeLevel)
                    .FirstOrDefaultAsync(sp => sp.UserId == userId && !sp.IsDeleted);

                if (studentProfile == null)
                {
                    _db.QrCodeLogs.Add(new QrCodeLog
                    {
                        QrCode = qrCode,
                        ScanTime = scanTime,
                        UserId = userId
                    });
                    await _db.SaveChangesAsync();
                    return new ScanResultDto { Success = false, ErrorMessage = "Student not found" };
                }

                // Step 3: Resolve session
                var session = await GetSessionForStudentAtTimeAsync(studentProfile.Id, scanTime);
                if (session == null)
                {
                    _db.QrCodeLogs.Add(new QrCodeLog
                    {
                        QrCode = qrCode,
                        ScanTime = scanTime,
                        UserId = studentProfile.UserId
                    });
                    await _db.SaveChangesAsync();
                    return new ScanResultDto { Success = false, ErrorMessage = "No active session found at this time" };
                }

                // Step 4: Load session with related data
                session = await _db.Sessions
                    .Include(s => s.Group)
                        .ThenInclude(g => g.Course)
                            .ThenInclude(c => c.Subject)
                    .Include(s => s.Group)
                        .ThenInclude(g => g.Course)
                            .ThenInclude(c => c.GradeLevel)
                    .Include(s => s.Group)
                        .ThenInclude(g => g.InstructorProfile)
                            .ThenInclude(ip => ip.User)
                    .FirstOrDefaultAsync(s => s.Id == session.Id);

                // Step 5: Check existing attendance for duplicate
                var existing = await _db.StudentAttendances
                    .FirstOrDefaultAsync(a =>
                        a.StudentProfileId == studentProfile.Id &&
                        a.SessionId == session!.Id &&
                        !a.IsDeleted);

                if (existing != null)
                {
                    _db.QrCodeLogs.Add(new QrCodeLog
                    {
                        QrCode = qrCode,
                        ScanTime = scanTime,
                        UserId = studentProfile.UserId
                    });
                    await _db.SaveChangesAsync();
                    return new ScanResultDto
                    {
                        Success = true,
                        StudentName = studentProfile.User.FullName,
                        StudentImagePath = studentProfile.User.ImagePath,
                        SessionTitle = $"{session!.Group.Course.Subject.Name} — {session.Group.Course.Name}",
                        GroupName = session.Group.Name,
                        InstructorName = session.Group.InstructorProfile.User.FullName,
                        GradeLevelName = session.Group.Course.GradeLevel.Name,
                        IsLate = existing.IsLate,
                        ScanTime = existing.ScanTime,
                        AttendanceId = existing.Id,
                        StudentProfileId = studentProfile.Id,
                        ErrorMessage = "Already scanned"
                    };
                }

                // Step 6: Compute IsLate
                var graceMinutes = _config.GetValue<int>("AppSettings:AttendanceLateGraceMinutes");
                var isLate = scanTime.TimeOfDay > session!.StartTime.Add(TimeSpan.FromMinutes(graceMinutes));

                // Step 7: Create StudentAttendance
                var attendance = new StudentAttendance
                {
                    StudentProfileId = studentProfile.Id,
                    SessionId = session.Id,
                    IsPresent = true,
                    IsLate = isLate,
                    ScanTime = scanTime
                };
                _db.StudentAttendances.Add(attendance);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
                {
                    return new ScanResultDto { Success = true, ErrorMessage = "Already scanned" };
                }

                // Step 8: Write QrCodeLog
                _db.QrCodeLogs.Add(new QrCodeLog
                {
                    QrCode = qrCode,
                    ScanTime = scanTime,
                    UserId = studentProfile.UserId
                });
                await _db.SaveChangesAsync();

                // Step 9: Return result
                return new ScanResultDto
                {
                    Success = true,
                    StudentName = studentProfile.User.FullName,
                    StudentImagePath = studentProfile.User.ImagePath,
                    SessionTitle = $"{session.Group.Course.Subject.Name} — {session.Group.Course.Name}",
                    GroupName = session.Group.Name,
                    InstructorName = session.Group.InstructorProfile.User.FullName,
                    GradeLevelName = session.Group.Course.GradeLevel.Name,
                    IsLate = isLate,
                    ScanTime = scanTime,
                    AttendanceId = attendance.Id,
                    StudentProfileId = studentProfile.Id
                };
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAsync(
                    "SYSTEM",
                    "ProcessScanError",
                    "QrCodeLog",
                    0,
                    null,
                    ex.Message);

                return new ScanResultDto { Success = false, ErrorMessage = "System error" };
            }
        }

        // =========================================
        // GetSessionSummaryAsync
        // =========================================

        public async Task<AttendanceSessionSummaryDto> GetSessionSummaryAsync(int sessionId)
        {
            var session = await _db.Sessions
                .Include(s => s.Group).ThenInclude(g => g.Enrollments)
                .Include(s => s.Attendances)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                return new AttendanceSessionSummaryDto { SessionId = sessionId };
            }

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
                AttendanceRatePercent = Math.Round(rate, 1)
            };
        }

        // =========================================
        // MarkManuallyAsync
        // =========================================

        public async Task MarkManuallyAsync(ManualMarkDto dto, string adminId)
        {
            var existing = await _db.StudentAttendances
                .FirstOrDefaultAsync(a =>
                    a.StudentProfileId == dto.StudentProfileId &&
                    a.SessionId == dto.SessionId &&
                    !a.IsDeleted);

            if (existing != null)
            {
                existing.IsPresent = dto.IsPresent;
                existing.IsLate = dto.IsLate;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.StudentAttendances.Add(new StudentAttendance
                {
                    StudentProfileId = dto.StudentProfileId,
                    SessionId = dto.SessionId,
                    IsPresent = dto.IsPresent,
                    IsLate = dto.IsLate,
                    ScanTime = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            await _auditLogService.LogAsync(
                adminId,
                "ManualAttendanceMark",
                "StudentAttendance",
                dto.SessionId,
                null,
                $"StudentProfileId={dto.StudentProfileId}, IsPresent={dto.IsPresent}, IsLate={dto.IsLate}");
        }

        // =========================================
        // GetSessionAttendanceListAsync
        // =========================================

        public async Task<PagedResult<AttendanceListItemDto>> GetSessionAttendanceListAsync(
            int sessionId, int page, int pageSize, string? search)
        {
            var query = _db.StudentAttendances
                .Include(a => a.StudentProfile)
                    .ThenInclude(sp => sp.User)
                .Where(a => a.SessionId == sessionId && !a.IsDeleted)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.StudentProfile.User.FullName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.ScanTime)
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

        // =========================================
        // GetStudentAttendanceHistoryAsync
        // =========================================

        public async Task<List<StudentAttendanceDto>> GetStudentAttendanceHistoryAsync(
            int studentProfileId, int? groupId, DateTime? from, DateTime? to)
        {
            var query = _db.StudentAttendances
                .Include(a => a.Session)
                    .ThenInclude(s => s.Group)
                        .ThenInclude(g => g.Course)
                            .ThenInclude(c => c.Subject)
                .Where(a => a.StudentProfileId == studentProfileId && !a.IsDeleted)
                .AsNoTracking();

            if (groupId.HasValue)
            {
                query = query.Where(a => a.Session.GroupId == groupId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(a => a.Session.SessionDate >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(a => a.Session.SessionDate <= to.Value);
            }

            return await query
                .OrderByDescending(a => a.Session.SessionDate)
                .Select(a => new StudentAttendanceDto
                {
                    AttendanceId = a.Id,
                    SessionDate = a.Session.SessionDate,
                    SubjectName = a.Session.Group.Course.Subject.Name,
                    GroupName = a.Session.Group.Name,
                    ScanTime = a.ScanTime,
                    IsPresent = a.IsPresent,
                    IsLate = a.IsLate
                })
                .ToListAsync();
        }

        // =========================================
        // GetStudentAttendanceRateAsync
        // =========================================

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
}
