namespace CenterManagement.Application.DTOs.Attendance
{
    public class ManualMarkDto
    {
        public int StudentProfileId { get; set; }
        public int SessionId { get; set; }
        public bool IsPresent { get; set; }
        public bool IsLate { get; set; }
    }
}
