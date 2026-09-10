using CenterManagement.Application.DTOs.Session;
using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CenterManagement.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly CenterManagementDbContext _db;
        private readonly IAuditLogService _audit;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SessionService(
            CenterManagementDbContext db,
            IAuditLogService audit,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _audit = audit;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // =========================================
        // CreateSessionAsync
        // =========================================

        public async Task<int> CreateSessionAsync(CreateSessionDto dto, string adminId)
        {
            // 1. Session date cannot be in the past
            if (dto.SessionDate.Date < DateTime.Now.Date)
                throw new InvalidOperationException("Session date cannot be in the past");

            // 2. Start time must be before end time
            if (dto.StartTime >= dto.EndTime)
                throw new InvalidOperationException("Start time must be before end time");

            // 3. Overlap check (Group Level)
            var overlap = await _db.Sessions
                .Where(s =>
                    s.GroupId == dto.GroupId &&
                    s.SessionDate.Date == dto.SessionDate.Date &&
                    !s.IsCanceled &&
                    !s.IsDeleted &&
                    (
                        (dto.StartTime >= s.StartTime && dto.StartTime < s.EndTime) ||
                        (dto.EndTime > s.StartTime && dto.EndTime <= s.EndTime) ||
                        (dto.StartTime <= s.StartTime && dto.EndTime >= s.EndTime)
                    )
                )
                .AnyAsync();

            if (overlap)
                throw new InvalidOperationException(
                    "An overlapping session already exists for this group on that date.");

            // 3.1 Overlap check (Instructor Level - GAP-006)
            var group = await _db.Groups.FindAsync(dto.GroupId);
            if (group != null)
            {
                var instructorOverlap = await _db.Sessions
                    .Include(s => s.Group)
                    .Where(s => s.Group.InstructorProfileId == group.InstructorProfileId
                        && s.SessionDate.Date == dto.SessionDate.Date
                        && !s.IsCanceled && !s.IsDeleted
                        && (
                            (dto.StartTime >= s.StartTime && dto.StartTime < s.EndTime) ||
                            (dto.EndTime > s.StartTime && dto.EndTime <= s.EndTime) ||
                            (dto.StartTime <= s.StartTime && dto.EndTime >= s.EndTime)
                        ))
                    .AnyAsync();

                if (instructorOverlap)
                    throw new InvalidOperationException(
                        "The instructor already has a session scheduled at this time.");
            }

            var session = new Session
            {
                GroupId = dto.GroupId,
                SessionDate = dto.SessionDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();

            // 4. Write audit
            await _audit.LogAsync(
                adminId,
                "SessionCreated",
                "Session",
                session.Id,
                null,
                JsonSerializer.Serialize(new { dto.GroupId, dto.SessionDate, dto.StartTime, dto.EndTime }));

            return session.Id;
        }

        // =========================================
        // GetSessionDetailAsync
        // =========================================

        public async Task<SessionDetailDto> GetSessionDetailAsync(int sessionId)
        {
            var session = await _db.Sessions
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.GradeLevel)
                .Include(s => s.Group).ThenInclude(g => g.InstructorProfile).ThenInclude(ip => ip.User)
                .Include(s => s.SubstituteInstructor).ThenInclude(si => si.User)
                .Include(s => s.Group).ThenInclude(g => g.Enrollments)
                .Include(s => s.Attendances)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId)
                ?? throw new KeyNotFoundException($"Session {sessionId} not found.");

            return new SessionDetailDto
            {
                Id = session.Id,
                SessionId = session.Id,
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                GroupName = session.Group.Name,
                CourseName = session.Group.Course.Name,
                SubjectName = session.Group.Course.Subject.Name,
                GradeLevelName = session.Group.Course.GradeLevel.Name,
                InstructorName = session.Group.InstructorProfile?.User?.FullName ?? "N/A",
                SubstituteInstructorName = session.SubstituteInstructor?.User?.FullName,
                IsCanceled = session.IsCanceled,
                CancelReason = session.CancelReason,
                AttendanceCount = session.Attendances.Count(a => a.IsPresent),
                GroupId = session.GroupId,
                InstructorProfileId = session.Group.InstructorProfileId,
                EnrolledCount = session.Group.Enrollments.Count(e => e.IsActive),
                AttendanceList = new()
            };
        }

        // =========================================
        // GetSessionsByGroupAsync
        // =========================================

        public async Task<List<SessionListItemDto>> GetSessionsByGroupAsync(int groupId, DateTime? from, DateTime? to)
        {
            var query = _db.Sessions
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.GradeLevel)
                .Include(s => s.Group).ThenInclude(g => g.InstructorProfile).ThenInclude(ip => ip.User)
                .Include(s => s.Attendances)
                .AsNoTracking()
                .Where(s => s.GroupId == groupId);

            if (from.HasValue)
                query = query.Where(s => s.SessionDate >= from.Value);

            if (to.HasValue)
                query = query.Where(s => s.SessionDate <= to.Value);

            var sessions = await query
                .OrderByDescending(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return sessions.Select(MapToListItem).ToList();
        }

        // =========================================
        // GetSessionsByDateAsync
        // =========================================

        public async Task<List<SessionListItemDto>> GetSessionsByDateAsync(DateTime date)
        {
            var sessions = await _db.Sessions
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.GradeLevel)
                .Include(s => s.Group).ThenInclude(g => g.InstructorProfile).ThenInclude(ip => ip.User)
                .Include(s => s.Attendances)
                .AsNoTracking()
                .Where(s => s.SessionDate.Date == date.Date)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return sessions.Select(MapToListItem).ToList();
        }

        // =========================================
        // GetSessionsByDateRangeAsync
        // =========================================

        public async Task<List<SessionListItemDto>> GetSessionsByDateRangeAsync(DateTime from, DateTime to)
        {
            var sessions = await _db.Sessions
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.GradeLevel)
                .Include(s => s.Group).ThenInclude(g => g.InstructorProfile).ThenInclude(ip => ip.User)
                .Include(s => s.Attendances)
                .AsNoTracking()
                .Where(s => s.SessionDate.Date >= from.Date && s.SessionDate.Date <= to.Date)
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return sessions.Select(MapToListItem).ToList();
        }

        // =========================================
        // CancelSessionAsync
        // =========================================

        public async Task CancelSessionAsync(int sessionId, string cancelReason, string performedByUserId)
        {
            // 1. Load session with Include(s => s.Group)
            var session = await _db.Sessions
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == sessionId)
                ?? throw new KeyNotFoundException($"Session {sessionId} not found.");

            // 2. If session.IsCanceled → throw
            if (session.IsCanceled)
                throw new InvalidOperationException("Session is already canceled");

            // 3. If caller has Instructor role → verify ownership
            var user = await _userManager.FindByIdAsync(performedByUserId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Instructor"))
            {
                var instructorProfileId = await GetInstructorProfileIdByUserIdAsync(performedByUserId);
                if (instructorProfileId != session.Group.InstructorProfileId)
                    throw new UnauthorizedAccessException("You do not own this session's group");
            }

            // 4. Set canceled
            session.IsCanceled = true;
            session.CancelReason = cancelReason;
            session.CanceledAt = DateTime.UtcNow; // GAP-009: 5-minute grace period
            session.UpdatedAt = DateTime.UtcNow;

            // 5. Save
            await _db.SaveChangesAsync();

            // 6. Write audit
            await _audit.LogAsync(
                performedByUserId,
                "SessionCanceled",
                "Session",
                sessionId,
                null,
                JsonSerializer.Serialize(new { cancelReason }));
                
            // Note: Notification sending is deferred for 5 minutes by background processor (GAP-009)
        }

        // =========================================
        // UncancelSessionAsync (GAP-009)
        // =========================================

        public async Task UncancelSessionAsync(int sessionId, string adminId)
        {
            var session = await _db.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId)
                ?? throw new KeyNotFoundException($"Session {sessionId} not found.");

            if (!session.IsCanceled)
                throw new InvalidOperationException("Session is not canceled.");

            if (session.CanceledAt == null || (DateTime.UtcNow - session.CanceledAt.Value).TotalMinutes > 5)
            {
                throw new InvalidOperationException("Cancellation grace period (5 minutes) has expired.");
            }

            session.IsCanceled = false;
            session.CancelReason = null;
            session.CanceledAt = null;
            session.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                adminId,
                "SessionUncanceled",
                "Session",
                sessionId,
                null,
                null);
        }

        // =========================================
        // AssignSubstituteAsync (GAP-007)
        // =========================================

        public async Task AssignSubstituteAsync(int sessionId, int substituteInstructorProfileId, string adminId)
        {
            var session = await _db.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId)
                ?? throw new KeyNotFoundException($"Session {sessionId} not found.");

            session.SubstituteInstructorProfileId = substituteInstructorProfileId;
            session.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _notificationService.SendToGroupAsync(
                session.GroupId,
                "Instructor Changed",
                $"A substitute instructor has been assigned for the session on {session.SessionDate:d}.");

            await _audit.LogAsync(
                adminId,
                "SubstituteAssigned",
                "Session",
                sessionId,
                null,
                JsonSerializer.Serialize(new { SubstituteInstructorProfileId = substituteInstructorProfileId }));
        }

        // =========================================
        // GetInstructorProfileIdByUserIdAsync
        // =========================================

        public async Task<int?> GetInstructorProfileIdByUserIdAsync(string userId)
        {
            return await _db.InstructorProfiles
                .Where(x => x.UserId == userId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        // =========================================
        // Private Helpers
        // =========================================

        private static SessionListItemDto MapToListItem(Session s)
        {
            return new SessionListItemDto
            {
                Id = s.Id,
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                GroupName = s.Group.Name,
                CourseName = s.Group.Course.Name,
                SubjectName = s.Group.Course.Subject.Name,
                GradeLevelName = s.Group.Course.GradeLevel.Name,
                InstructorName = s.Group.InstructorProfile?.User?.FullName ?? "N/A",
                IsCanceled = s.IsCanceled,
                CancelReason = s.CancelReason,
                AttendanceCount = s.Attendances.Count(a => a.IsPresent)
            };
        }
    }
}
