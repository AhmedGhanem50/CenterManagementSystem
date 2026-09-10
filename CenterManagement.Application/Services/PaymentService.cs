using CenterManagement.Application.DTOs.Common;
using CenterManagement.Application.DTOs.Payment;
using CenterManagement.Application.DTOs.Student;
using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace CenterManagement.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly CenterManagementDbContext _db;
        private readonly IAuditLogService _audit;

        public PaymentService(CenterManagementDbContext db, IAuditLogService audit)
        {
            _db = db;
            _audit = audit;
        }

        // =========================================
        // CreateCoursePaymentAsync
        // =========================================

        public async Task<StudentCoursePayment> CreateCoursePaymentAsync(
            int studentProfileId, int courseId, string adminId)
        {
            // Check no existing non-deleted StudentCoursePayment for same student + course
            var exists = await _db.StudentCoursePayments
                .AnyAsync(scp =>
                    scp.StudentProfileId == studentProfileId &&
                    scp.CourseId == courseId);

            if (exists)
                throw new InvalidOperationException(
                    "Course payment already exists for this student");

            var course = await _db.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId)
                ?? throw new KeyNotFoundException($"Course {courseId} not found.");

            var scp = new StudentCoursePayment
            {
                StudentProfileId = studentProfileId,
                CourseId = courseId,
                RequiredAmount = course.Price,
                PaidAmount = 0,
                RemainingAmount = course.Price,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.StudentCoursePayments.Add(scp);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                adminId,
                "CoursePaymentCreated",
                "StudentCoursePayment",
                scp.Id,
                null,
                JsonSerializer.Serialize(new
                {
                    studentProfileId,
                    courseId,
                    scp.RequiredAmount
                }));

            return scp;
        }

        // =========================================
        // RecordPaymentAsync
        // =========================================

        public async Task<PaymentTransaction> RecordPaymentAsync(
            RecordPaymentDto dto, string adminId)
        {
            // Load with tracking (no AsNoTracking)
            var scp = await _db.StudentCoursePayments
                .FirstOrDefaultAsync(s => s.Id == dto.StudentCoursePaymentId)
                ?? throw new KeyNotFoundException(
                    $"StudentCoursePayment {dto.StudentCoursePaymentId} not found.");

            if (scp.IsPaid)
                throw new InvalidOperationException(
                    "This course is already fully paid");

            if (dto.Amount <= 0)
                throw new InvalidOperationException(
                    "Amount must be greater than zero");

            if (dto.Amount > scp.RemainingAmount)
                throw new InvalidOperationException(
                    $"Amount exceeds remaining balance of {scp.RemainingAmount}");

            // Update payment amounts
            scp.PaidAmount += dto.Amount;
            scp.RemainingAmount = scp.RequiredAmount - scp.PaidAmount; // always recompute

            if (scp.RemainingAmount <= 0)
            {
                scp.IsPaid = true;
                scp.RemainingAmount = 0;
            }

            scp.UpdatedAt = DateTime.UtcNow;

            // Create transaction — same SaveChangesAsync call as scp update
            var tx = new PaymentTransaction
            {
                StudentCoursePaymentId = scp.Id,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
                AdminId = adminId,
                CreatedAt = DateTime.UtcNow
            };

            _db.PaymentTransactions.Add(tx);

            // Atomic save — scp update + tx creation in ONE call
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                adminId,
                "PaymentRecorded",
                "PaymentTransaction",
                tx.Id,
                null,
                $"Amount:{dto.Amount},Remaining:{scp.RemainingAmount}");

            return tx;
        }

        // =========================================
        // VoidTransactionAsync (GAP-005)
        // =========================================

        public async Task VoidTransactionAsync(int transactionId, string reason, string adminId)
        {
            var tx = await _db.PaymentTransactions
                .Include(t => t.StudentCoursePayment)
                .FirstOrDefaultAsync(t => t.Id == transactionId)
                ?? throw new KeyNotFoundException($"PaymentTransaction {transactionId} not found.");

            if (tx.IsSoftDeleted)
                throw new InvalidOperationException("Transaction is already voided.");

            // Mark as soft deleted
            tx.IsSoftDeleted = true;
            tx.DeletionReason = reason;
            tx.DeletedAt = DateTime.UtcNow;
            tx.UpdatedAt = DateTime.UtcNow;

            // Recalculate parent payment
            var scp = tx.StudentCoursePayment;
            scp.PaidAmount -= tx.Amount;
            
            // Cannot have negative paid amount due to voiding
            if (scp.PaidAmount < 0) scp.PaidAmount = 0;
            
            scp.RemainingAmount = scp.RequiredAmount - scp.PaidAmount;
            scp.IsPaid = scp.RemainingAmount <= 0;
            scp.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                adminId,
                "PaymentVoided",
                "PaymentTransaction",
                tx.Id,
                null,
                JsonSerializer.Serialize(new { reason, AmountVoided = tx.Amount, NewRemaining = scp.RemainingAmount }));
        }

        // =========================================
        // CreateSessionPaymentAsync
        // =========================================

        public async Task<SessionPayment> CreateSessionPaymentAsync(
            CreateSessionPaymentDto dto, string adminId)
        {
            var studentExists = await _db.StudentProfiles
                .AnyAsync(s => s.Id == dto.StudentProfileId);

            if (!studentExists)
                throw new KeyNotFoundException(
                    $"StudentProfile {dto.StudentProfileId} not found.");

            var sessionExists = await _db.Sessions
                .AnyAsync(s => s.Id == dto.SessionId);

            if (!sessionExists)
                throw new KeyNotFoundException(
                    $"Session {dto.SessionId} not found.");

            var sp = new SessionPayment
            {
                StudentProfileId = dto.StudentProfileId,
                SessionId = dto.SessionId,
                Amount = dto.Amount,
                AdminId = adminId,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.SessionPayments.Add(sp);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                adminId,
                "SessionPaymentCreated",
                "SessionPayment",
                sp.Id,
                null,
                JsonSerializer.Serialize(new
                {
                    dto.StudentProfileId,
                    dto.SessionId,
                    dto.Amount
                }));

            return sp;
        }

        // =========================================
        // GetStudentFinancialSummaryAsync
        // =========================================

        public async Task<StudentFinancialSummaryDto> GetStudentFinancialSummaryAsync(
            int studentProfileId)
        {
            var student = await _db.StudentProfiles
                .IgnoreQueryFilters()
                .Include(s => s.User)
                .Include(s => s.CoursePayments)
                    .ThenInclude(cp => cp.Course)
                .Include(s => s.SessionPayments)
                    .ThenInclude(sp => sp.Session)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentProfileId)
                ?? throw new KeyNotFoundException(
                    $"StudentProfile {studentProfileId} not found.");

            var coursePayments = student.CoursePayments
                .Select(cp => new StudentCoursePaymentSummaryDto
                {
                    StudentCoursePaymentId = cp.Id,
                    CourseName = cp.Course.Name,
                    RequiredAmount = cp.RequiredAmount,
                    PaidAmount = cp.PaidAmount,
                    RemainingAmount = cp.RemainingAmount,
                    IsPaid = cp.IsPaid
                })
                .ToList();

            var sessionPayments = student.SessionPayments
                .Select(sp => new SessionPaymentDto
                {
                    SessionPaymentId = sp.Id,
                    SessionTitle = $"{sp.Session.Group.Course.Name} - {sp.Session.SessionDate:yyyy-MM-dd}",
                    Amount = sp.Amount,
                    PaymentDate = sp.PaymentDate,
                    PaymentMethod = "Cash" // SessionPayment entity doesn't store method
                })
                .ToList();

            var unbilledEnrollments = await _db.Enrollments
                .Include(e => e.Group).ThenInclude(g => g.Course)
                .Where(e => e.StudentProfileId == studentProfileId && e.IsActive)
                .Where(e => !_db.StudentCoursePayments.Any(cp => cp.StudentProfileId == studentProfileId && cp.CourseId == e.Group.CourseId))
                .Select(e => new UnbilledEnrollmentDto
                {
                    EnrollmentId = e.Id,
                    CourseId = e.Group.CourseId,
                    CourseName = e.Group.Course.Name,
                    Price = e.Group.Course.Price
                })
                .Distinct()
                .ToListAsync();

            var totalRequired = coursePayments.Sum(cp => cp.RequiredAmount);
            var totalPaid = coursePayments.Sum(cp => cp.PaidAmount);

            return new StudentFinancialSummaryDto
            {
                StudentProfileId = studentProfileId,
                StudentName = student.User.FullName,
                TotalRequired = totalRequired,
                TotalPaid = totalPaid,
                TotalRemaining = totalRequired - totalPaid,
                IsFullyPaid = coursePayments.All(cp => cp.IsPaid),
                CoursePayments = coursePayments,
                SessionPayments = sessionPayments,
                UnbilledEnrollments = unbilledEnrollments
            };
        }

        // =========================================
        // GetTransactionListAsync
        // =========================================

        public async Task<PagedResult<TransactionListItemDto>> GetTransactionListAsync(
            TransactionFilter filter)
        {
            var query = _db.PaymentTransactions
                .IgnoreQueryFilters()
                .Include(t => t.StudentCoursePayment)
                    .ThenInclude(scp => scp.StudentProfile)
                        .ThenInclude(sp => sp.User)
                .Include(t => t.StudentCoursePayment)
                    .ThenInclude(scp => scp.Course)
                .AsNoTracking()
                .AsQueryable();

            // Filter by student name
            if (!string.IsNullOrWhiteSpace(filter.StudentName))
                query = query.Where(t =>
                    t.StudentCoursePayment.StudentProfile.User.FullName
                        .Contains(filter.StudentName));

            // Filter by status
            if (filter.PaymentStatus == "Paid")
                query = query.Where(t => t.StudentCoursePayment.IsPaid);
            else if (filter.PaymentStatus == "Pending")
                query = query.Where(t => !t.StudentCoursePayment.IsPaid);

            // Filter by date range
            if (filter.DateFrom.HasValue)
                query = query.Where(t => t.PaymentDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                query = query.Where(t => t.PaymentDate <= filter.DateTo.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.PaymentDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new TransactionListItemDto
                {
                    TransactionId = t.Id,
                    PaymentDate = t.PaymentDate,
                    StudentName = t.StudentCoursePayment.StudentProfile.User.FullName,
                    StudentImagePath = t.StudentCoursePayment.StudentProfile.User.ImagePath,
                    CourseName = t.StudentCoursePayment.Course.Name,
                    Amount = t.Amount,
                    PaymentMethod = "Cash", // PaymentTransaction doesn't store method
                    IsPaid = t.StudentCoursePayment.IsPaid,
                    RemainingAfterPayment = t.StudentCoursePayment.RemainingAmount
                })
                .ToListAsync();

            return new PagedResult<TransactionListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        // =========================================
        // GetPaymentKpisAsync — server-side aggregation only
        // =========================================

        public async Task<PaymentKpiDto> GetPaymentKpisAsync()
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var totalDueToday = await _db.PaymentTransactions
                .Where(t => t.PaymentDate.Date == today && !t.IsDeleted)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var revenueThisMonth = await _db.PaymentTransactions
                .Where(t => t.PaymentDate >= monthStart && !t.IsDeleted)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalRevenue = await _db.PaymentTransactions
                .Where(t => !t.IsDeleted)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var outstanding = await _db.StudentCoursePayments
                .CountAsync(s => !s.IsPaid && !s.IsDeleted);

            var paid = await _db.StudentCoursePayments
                .CountAsync(s => s.IsPaid && !s.IsDeleted);

            var totalRequired = await _db.StudentCoursePayments
                .Where(s => !s.IsDeleted)
                .SumAsync(s => (decimal?)s.RequiredAmount) ?? 0;

            var totalPaid = await _db.StudentCoursePayments
                .Where(s => !s.IsDeleted)
                .SumAsync(s => (decimal?)s.PaidAmount) ?? 0;

            var collectionRate = totalRequired > 0
                ? totalPaid / totalRequired * 100
                : 0;

            return new PaymentKpiDto
            {
                TotalDueToday = totalDueToday,
                RevenueThisMonth = revenueThisMonth,
                TotalRevenueAllTime = totalRevenue,
                OutstandingStudentsCount = outstanding,
                CollectionRatePercent = collectionRate,
                PaidStudentsCount = paid
            };
        }

        // =========================================
        // GetOutstandingStudentsAsync
        // =========================================

        public async Task<List<OutstandingStudentDto>> GetOutstandingStudentsAsync()
        {
            return await _db.StudentCoursePayments
                .Where(scp => !scp.IsPaid && !scp.IsDeleted)
                .GroupBy(scp => new
                {
                    scp.StudentProfileId,
                    scp.StudentProfile.User.FullName,
                    scp.StudentProfile.User.ImagePath,
                    GradeLevelName = scp.StudentProfile.GradeLevel.Name
                })
                .Select(g => new OutstandingStudentDto
                {
                    StudentProfileId = g.Key.StudentProfileId,
                    StudentName = g.Key.FullName,
                    ImagePath = g.Key.ImagePath,
                    GradeLevelName = g.Key.GradeLevelName,
                    TotalRemaining = g.Sum(scp => scp.RemainingAmount)
                })
                .OrderByDescending(s => s.TotalRemaining)
                .ToListAsync();
        }

        // =========================================
        // ExportTransactionsCsvAsync
        // =========================================

        public async Task<byte[]> ExportTransactionsCsvAsync(TransactionFilter filter)
        {
            var transactions = await GetTransactionListAsync(new TransactionFilter
            {
                StudentName = filter.StudentName,
                PaymentStatus = filter.PaymentStatus,
                DateFrom = filter.DateFrom,
                DateTo = filter.DateTo,
                Page = 1,
                PageSize = int.MaxValue
            });

            var sb = new StringBuilder();
            sb.AppendLine("Date,Student,Course,Amount,Method,Status,Remaining");

            foreach (var t in transactions.Items)
            {
                sb.AppendLine(
                    $"{t.PaymentDate:yyyy-MM-dd},{t.StudentName}," +
                    $"{t.CourseName},{t.Amount},{t.PaymentMethod}," +
                    $"{(t.IsPaid ? "Paid" : "Pending")},{t.RemainingAfterPayment}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
