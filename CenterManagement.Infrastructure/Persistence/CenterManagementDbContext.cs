using CenterManagement.Domain.Common;
using CenterManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CenterManagement.Infrastructure.Persistence
{
    public class CenterManagementDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public CenterManagementDbContext(
            DbContextOptions<CenterManagementDbContext> options)
            : base(options)
        {
        }

        // =========================================
        // Academic
        // =========================================

        public DbSet<GradeLevel> GradeLevels { get; set; }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        // =========================================
        // Profiles
        // =========================================

        public DbSet<StudentProfile> StudentProfiles { get; set; }

        public DbSet<InstructorProfile> InstructorProfiles { get; set; }

        // =========================================
        // Attendance
        // =========================================

        public DbSet<StudentAttendance> StudentAttendances { get; set; }

        public DbSet<InstructorAttendance> InstructorAttendances { get; set; }

        public DbSet<QrCodeLog> QrCodeLogs { get; set; }

        // =========================================
        // Payments
        // =========================================

        public DbSet<StudentCoursePayment> StudentCoursePayments { get; set; }

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<SessionPayment> SessionPayments { get; set; }

        // =========================================
        // Notifications
        // =========================================

        public DbSet<Notification> Notifications { get; set; }

        // =========================================
        // Settings
        // =========================================

        public DbSet<SystemSetting> SystemSettings { get; set; }

        // =========================================
        // Audit
        // =========================================

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =========================================
            // Disable Cascade Delete
            // =========================================

            foreach (var relationship in builder.Model
                         .GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior =
                    DeleteBehavior.Restrict;
            }

            // =========================================
            // Global Soft Delete Filter
            // =========================================

            ApplySoftDeleteFilter(builder);

            // =========================================
            // Decimal Precision
            // =========================================

            builder.Entity<Course>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Entity<StudentCoursePayment>()
                .Property(x => x.RequiredAmount)
                .HasPrecision(18, 2);

            builder.Entity<StudentCoursePayment>()
                .Property(x => x.PaidAmount)
                .HasPrecision(18, 2);

            builder.Entity<StudentCoursePayment>()
                .Property(x => x.RemainingAmount)
                .HasPrecision(18, 2);

            builder.Entity<PaymentTransaction>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Entity<SessionPayment>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            // =========================================
            // Unique Constraints
            // =========================================

            builder.Entity<GradeLevel>()
                .HasIndex(x => x.Name)
                .IsUnique();

            builder.Entity<Subject>()
                .HasIndex(x => x.Name)
                .IsUnique();

            builder.Entity<Enrollment>()
                .HasIndex(x => new
                {
                    x.StudentProfileId,
                    x.GroupId
                })
                .IsUnique();

            builder.Entity<StudentAttendance>()
                .HasIndex(x => new
                {
                    x.StudentProfileId,
                    x.SessionId
                })
                .IsUnique();

            // =========================================
            // ApplicationUser -> StudentProfile
            // One To One
            // =========================================

            builder.Entity<StudentProfile>()
                .HasOne(x => x.User)
                .WithOne(x => x.StudentProfile)
                .HasForeignKey<StudentProfile>(x => x.UserId);

            // =========================================
            // ApplicationUser -> InstructorProfile
            // One To One
            // =========================================

            builder.Entity<InstructorProfile>()
                .HasOne(x => x.User)
                .WithOne(x => x.InstructorProfile)
                .HasForeignKey<InstructorProfile>(x => x.UserId);

            // =========================================
            // Subject -> Instructors
            // =========================================

            builder.Entity<InstructorProfile>()
                .HasOne(x => x.Subject)
                .WithMany(x => x.Instructors)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================
            // GradeLevel -> Students
            // =========================================

            builder.Entity<StudentProfile>()
                .HasOne(x => x.GradeLevel)
                .WithMany(x => x.Students)
                .HasForeignKey(x => x.GradeLevelId);

            // =========================================
            // GradeLevel -> Courses
            // =========================================

            builder.Entity<Course>()
                .HasOne(x => x.GradeLevel)
                .WithMany(x => x.Courses)
                .HasForeignKey(x => x.GradeLevelId);

            // =========================================
            // Subject -> Courses
            // =========================================

            builder.Entity<Course>()
                .HasOne(x => x.Subject)
                .WithMany(x => x.Courses)
                .HasForeignKey(x => x.SubjectId);

            // =========================================
            // Course -> Groups
            // =========================================

            builder.Entity<Group>()
                .HasOne(x => x.Course)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.CourseId);

            // =========================================
            // Instructor -> Groups
            // =========================================

            builder.Entity<Group>()
                .HasOne(x => x.InstructorProfile)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.InstructorProfileId);

            // =========================================
            // Group -> Sessions
            // =========================================

            builder.Entity<Session>()
                .HasOne(x => x.Group)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.GroupId);

            // =========================================
            // Substitute Instructor -> Sessions
            // =========================================

            builder.Entity<Session>()
                .HasOne(x => x.SubstituteInstructor)
                .WithMany()
                .HasForeignKey(x => x.SubstituteInstructorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================
            // Student -> Enrollments
            // =========================================

            builder.Entity<Enrollment>()
                .HasOne(x => x.StudentProfile)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentProfileId);

            // =========================================
            // Group -> Enrollments
            // =========================================

            builder.Entity<Enrollment>()
                .HasOne(x => x.Group)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.GroupId);

            // =========================================
            // Student Attendance
            // =========================================

            builder.Entity<StudentAttendance>()
                .HasOne(x => x.StudentProfile)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.StudentProfileId);

            builder.Entity<StudentAttendance>()
                .HasOne(x => x.Session)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.SessionId);

            // =========================================
            // Instructor Attendance
            // =========================================

            builder.Entity<InstructorAttendance>()
                .HasOne(x => x.InstructorProfile)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.InstructorProfileId);

            // =========================================
            // Student -> Course Payments
            // =========================================

            builder.Entity<StudentCoursePayment>()
                .HasOne(x => x.StudentProfile)
                .WithMany(x => x.CoursePayments)
                .HasForeignKey(x => x.StudentProfileId);

            // =========================================
            // Course -> Student Payments
            // =========================================

            builder.Entity<StudentCoursePayment>()
                .HasOne(x => x.Course)
                .WithMany(x => x.StudentPayments)
                .HasForeignKey(x => x.CourseId);

            // =========================================
            // PaymentTransaction -> StudentCoursePayment
            // =========================================

            builder.Entity<PaymentTransaction>()
                .HasOne(x => x.StudentCoursePayment)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.StudentCoursePaymentId);

            // =========================================
            // PaymentTransaction -> Admin
            // =========================================

            builder.Entity<PaymentTransaction>()
                .HasOne(x => x.Admin)
                .WithMany(x => x.PaymentTransactions)
                .HasForeignKey(x => x.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================
            // SessionPayment -> Student
            // =========================================

            builder.Entity<SessionPayment>()
                .HasOne(x => x.StudentProfile)
                .WithMany(x => x.SessionPayments)
                .HasForeignKey(x => x.StudentProfileId);

            // =========================================
            // SessionPayment -> Session
            // =========================================

            builder.Entity<SessionPayment>()
                .HasOne(x => x.Session)
                .WithMany(x => x.SessionPayments)
                .HasForeignKey(x => x.SessionId);

            // =========================================
            // SessionPayment -> Admin
            // =========================================

            builder.Entity<SessionPayment>()
                .HasOne(x => x.Admin)
                .WithMany(x => x.SessionPayments)
                .HasForeignKey(x => x.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================
            // Notifications
            // =========================================

            builder.Entity<Notification>()
                .HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId);

            // =========================================
            // Audit Logs
            // =========================================

            builder.Entity<AuditLog>()
                .HasOne(x => x.User)
                .WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.UserId);

            // =========================================
            // QR Logs
            // =========================================

            builder.Entity<QrCodeLog>()
                .HasOne(x => x.User)
                .WithMany(x => x.QrCodeLogs)
                .HasForeignKey(x => x.UserId);

            // =========================================
            // Performance Indexes
            // =========================================

            builder.Entity<Session>()
                .HasIndex(x => x.SessionDate);

            builder.Entity<StudentAttendance>()
                .HasIndex(x => x.ScanTime);

            builder.Entity<PaymentTransaction>()
                .HasIndex(x => x.PaymentDate);

            builder.Entity<Notification>()
                .HasIndex(x => x.IsRead);
        }

        // =========================================
        // Global Soft Delete
        // =========================================

        private void ApplySoftDeleteFilter(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model
                         .GetEntityTypes())
            {
                if (typeof(BaseEntity)
                    .IsAssignableFrom(entityType.ClrType))
                {
                    var parameter =
                        Expression.Parameter(
                            entityType.ClrType,
                            "e");

                    var body = Expression.Equal(
                        Expression.Property(
                            parameter,
                            "IsDeleted"),
                        Expression.Constant(false));

                    var lambda =
                        Expression.Lambda(
                            body,
                            parameter);

                    builder.Entity(entityType.ClrType)
                        .HasQueryFilter(lambda);
                }
            }
        }

        // =========================================
        // Auto Update UpdatedAt
        // =========================================

        public override int SaveChanges()
        {
            UpdateTimestamps();

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();

            return await base.SaveChangesAsync(
                cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries =
                ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt =
                        DateTime.UtcNow;
                }
            }
        }
    }
}