using CenterManagement.Application.DTOs.Dashboard;
using CenterManagement.Application.DTOs.Session;
using CenterManagement.Application.Interfaces;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CenterManagement.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly CenterManagementDbContext _db;

        public DashboardService(CenterManagementDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardKpiDto> GetReceptionistKpisAsync()
        {
            var today = DateTime.Now.Date;
            var nowTime = DateTime.Now.TimeOfDay;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var totalStudents = await _db.StudentProfiles.CountAsync();

            var activeSessionsNow = await _db.Sessions
                .CountAsync(s =>
                    s.SessionDate.Date == today &&
                    !s.IsCanceled &&
                    s.StartTime <= nowTime &&
                    s.EndTime >= nowTime);

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var totalRecent = await _db.StudentAttendances
                .CountAsync(a => a.CreatedAt >= sevenDaysAgo);
            var presentRecent = await _db.StudentAttendances
                .CountAsync(a => a.CreatedAt >= sevenDaysAgo && a.IsPresent);
            var attendanceRate = totalRecent > 0
                ? (decimal)presentRecent / totalRecent * 100 : 0;

            var revenueToday = await _db.PaymentTransactions
                .Where(t => t.PaymentDate.Date == today)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var newStudentsThisMonth = await _db.StudentProfiles
                .CountAsync(s => s.CreatedAt >= monthStart);

            var failedNotifications = await _db.Notifications
                .CountAsync(n => !n.IsSent);

            return new DashboardKpiDto
            {
                TotalStudents = totalStudents,
                ActiveSessionsNow = activeSessionsNow,
                AttendanceRateLast7Days = attendanceRate,
                RevenueTodayAmount = revenueToday,
                NewStudentsThisMonth = newStudentsThisMonth,
                FailedNotificationCount = failedNotifications
            };
        }

        public async Task<List<ActiveSessionDto>> GetActiveSessionsAsync(DateTime now)
        {
            var nowTime = now.TimeOfDay;
            var today = now.Date;

            return await _db.Sessions
                .Where(s =>
                    s.SessionDate.Date == today &&
                    !s.IsCanceled &&
                    s.StartTime <= nowTime &&
                    s.EndTime >= nowTime)
                .Select(s => new ActiveSessionDto
                {
                    SessionId = s.Id,
                    CourseTitle = s.Group.Course.Name,
                    GroupName = s.Group.Name,
                    GradeLevelName = s.Group.Course.GradeLevel.Name,
                    InstructorName = s.Group.InstructorProfile.User.FullName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    TotalEnrolled = s.Group.Enrollments.Count(e => e.IsActive),
                    PresentCount = s.Attendances.Count(a => a.IsPresent)
                })
                .ToListAsync();
        }

        public async Task<List<SessionScheduleDto>> GetTodayScheduleAsync(DateTime today)
        {
            return await _db.Sessions
                .Where(s => s.SessionDate.Date == today.Date)
                .OrderBy(s => s.StartTime)
                .Select(s => new SessionScheduleDto
                {
                    SessionId = s.Id,
                    CourseTitle = s.Group.Course.Name,
                    GroupName = s.Group.Name,
                    InstructorName = s.Group.InstructorProfile.User.FullName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsCanceled = s.IsCanceled,
                    EnrolledCount = s.Group.Enrollments.Count(e => e.IsActive)
                })
                .ToListAsync();
        }
    }
}
