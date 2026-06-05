namespace CenterManagement.Application.DTOs.Attendance
{
    public class AttendanceSessionSummaryDto
    {
        public int SessionId { get; set; }
        public int TotalEnrolled { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public decimal AttendanceRatePercent { get; set; }
    }
}
