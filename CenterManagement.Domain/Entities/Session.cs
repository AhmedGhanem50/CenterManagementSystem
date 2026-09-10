using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class Session : BaseEntity
    {
        public int GroupId { get; set; }

        public Group Group { get; set; } = null!;

        public DateTime SessionDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsCanceled { get; set; } = false;

        public string? CancelReason { get; set; }

        public ICollection<StudentAttendance> Attendances
        { get; set; } = new List<StudentAttendance>();

        public ICollection<SessionPayment> SessionPayments
        { get; set; } = new List<SessionPayment>();

        // Substitute Instructor (GAP-007)
        public int? SubstituteInstructorProfileId { get; set; }
        public InstructorProfile? SubstituteInstructor { get; set; }

        // Grace Period Cancellation (GAP-009)
        public DateTime? CanceledAt { get; set; }
    }
}