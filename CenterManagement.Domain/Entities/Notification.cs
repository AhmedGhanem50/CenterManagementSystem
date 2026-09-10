using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; }
            = DateTime.UtcNow;

        // Retry Queue (GAP-012)
        public bool IsSent { get; set; } = true;
        public int RetryCount { get; set; } = 0;
        public string? FailureReason { get; set; }
        public DateTime? NextRetryAt { get; set; }
    }
}