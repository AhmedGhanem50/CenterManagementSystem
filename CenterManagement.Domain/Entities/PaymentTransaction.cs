using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public int StudentCoursePaymentId { get; set; }

        public StudentCoursePayment StudentCoursePayment
        { get; set; } = null!;

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }
            = DateTime.UtcNow;

        // =========================
        // Admin
        // =========================

        public string AdminId { get; set; } = string.Empty;

        public ApplicationUser Admin { get; set; } = null!;

        // Soft Delete (GAP-005)
        public bool IsSoftDeleted { get; set; } = false;
        public string? DeletionReason { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}