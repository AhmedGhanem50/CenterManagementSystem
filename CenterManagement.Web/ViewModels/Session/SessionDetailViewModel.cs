namespace CenterManagement.Web.ViewModels.Session
{
    public class SessionDetailViewModel
    {
        public int SessionId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string GradeLevelName { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsCanceled { get; set; }
        public string? CancelReason { get; set; }
    }
}
