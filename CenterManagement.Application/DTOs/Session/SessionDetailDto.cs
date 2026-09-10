namespace CenterManagement.Application.DTOs.Session
{
    public class SessionDetailDto
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string GradeLevelName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string? SubstituteInstructorName { get; set; }
        public bool IsCanceled { get; set; }
        public string? CancelReason { get; set; }
        public int AttendanceCount { get; set; }
        public int SessionId { get; set; }
        public int GroupId { get; set; }
        public int InstructorProfileId { get; set; }
        public int EnrolledCount { get; set; }
        // Phase 4 appends attendance list — declare as empty list here:
        public List<object> AttendanceList { get; set; } = new();
    }
}
