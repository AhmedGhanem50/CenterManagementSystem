namespace CenterManagement.Application.DTOs.Attendance
{
    public class ScanResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? StudentName { get; set; }
        public string? StudentImagePath { get; set; }
        public string? SessionTitle { get; set; }
        public string? GroupName { get; set; }
        public string? InstructorName { get; set; }
        public string? GradeLevelName { get; set; }
        public bool IsLate { get; set; }
        public DateTime? ScanTime { get; set; }
        public int? AttendanceId { get; set; }
        public int? StudentProfileId { get; set; }
    }
}
