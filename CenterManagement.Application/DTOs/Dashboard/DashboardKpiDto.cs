namespace CenterManagement.Application.DTOs.Dashboard
{
    public class DashboardKpiDto
    {
        public int TotalStudents { get; set; }
        public int ActiveSessionsNow { get; set; }
        public decimal AttendanceRateLast7Days { get; set; }
        public decimal RevenueTodayAmount { get; set; }
        public int NewStudentsThisMonth { get; set; }
        public int FailedNotificationCount { get; set; }
    }
}
