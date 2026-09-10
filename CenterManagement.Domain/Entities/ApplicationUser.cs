using Microsoft.AspNetCore.Identity;

namespace CenterManagement.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {   
        public string FullName { get; set; }
            = string.Empty;

        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        // =========================
        // Navigation Properties
        // =========================

        public StudentProfile? StudentProfile { get; set; }

        public InstructorProfile? InstructorProfile { get; set; }

        public ICollection<PaymentTransaction>
            PaymentTransactions
        { get; set; }
                = new List<PaymentTransaction>();

        public ICollection<SessionPayment>
            SessionPayments
        { get; set; }
                = new List<SessionPayment>();

        public ICollection<Notification>
            Notifications
        { get; set; }
                = new List<Notification>();

        public ICollection<AuditLog>
            AuditLogs
        { get; set; }
                = new List<AuditLog>();

        public ICollection<QrCodeLog>
            QrCodeLogs
        { get; set; }
                = new List<QrCodeLog>();
    }
}
