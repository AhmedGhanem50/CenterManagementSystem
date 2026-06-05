namespace CenterManagement.Application.DTOs.Attendance
{
    public class AttendanceListItemDto
    {
        public int AttendanceId { get; set; }
        public int StudentProfileId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentImagePath { get; set; }
        public bool IsPresent { get; set; }
        public bool IsLate { get; set; }
        public DateTime ScanTime { get; set; }
    }
}
