using CenterManagement.Application.DTOs.Common;
using CenterManagement.Application.DTOs.Student;
using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CenterManagement.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly CenterManagementDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;
        private readonly IFileUploadService _fileUpload;

        public StudentService(
            CenterManagementDbContext db,
            UserManager<ApplicationUser> userManager,
            IAuditLogService audit,
            IFileUploadService fileUpload)
        {
            _db = db;
            _userManager = userManager;
            _audit = audit;
            _fileUpload = fileUpload;
        }

        // =========================================
        // CreateStudentAsync
        // =========================================

        public async Task<int> CreateStudentAsync(CreateStudentDto dto, string adminId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            ApplicationUser? user = null;

            try
            {
                // 1. Create ApplicationUser
                var defaultPassword = "Student@" + dto.ParentPhone[^4..];

                user = new ApplicationUser
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user: {errors}");
                }

                // 2. Assign Student role
                await _userManager.AddToRoleAsync(user, "Student");

                // 3. Upload photo if provided
                if (dto.Photo != null)
                {
                    user.ImagePath = await _fileUpload.UploadPhotoAsync(dto.Photo, "students");
                    await _userManager.UpdateAsync(user);
                }

                // 4. Create StudentProfile
                var profile = new StudentProfile
                {
                    UserId = user.Id,
                    GradeLevelId = dto.GradeLevelId,
                    ParentPhone = dto.ParentPhone
                };
                _db.StudentProfiles.Add(profile);
                await _db.SaveChangesAsync();

                // 5. Create Enrollments
                foreach (var groupId in dto.GroupIds)
                {
                    _db.Enrollments.Add(new Enrollment
                    {
                        StudentProfileId = profile.Id,
                        GroupId = groupId,
                        IsActive = true,
                        EnrollmentDate = DateTime.UtcNow
                    });
                }

                await _db.SaveChangesAsync();

                // 6. Commit transaction
                await transaction.CommitAsync();

                // 7. Audit
                await _audit.LogAsync(
                    adminId,
                    "StudentCreated",
                    "StudentProfile",
                    profile.Id,
                    null,
                    JsonSerializer.Serialize(new { dto.FullName, dto.Email, dto.GradeLevelId, dto.GroupIds }));

                return profile.Id;
            }
            catch
            {
                await transaction.RollbackAsync();

                // Clean up orphaned user if it was created
                if (user != null && !string.IsNullOrEmpty(user.Id))
                {
                    var existingUser = await _userManager.FindByIdAsync(user.Id);
                    if (existingUser != null)
                    {
                        await _userManager.DeleteAsync(existingUser);
                    }
                }

                throw;
            }
        }

        // =========================================
        // GetStudentListAsync
        // =========================================

        public async Task<PagedResult<StudentListItemDto>> GetStudentListAsync(StudentListFilter filter)
        {
            var query = _db.StudentProfiles
                .Include(x => x.User)
                .Include(x => x.GradeLevel)
                .Include(x => x.Enrollments).ThenInclude(e => e.Group).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
                .Include(x => x.CoursePayments)
                .AsNoTracking();

            // Search filter
            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                query = query.Where(x =>
                    x.User.FullName.Contains(filter.SearchQuery) ||
                    x.User.PhoneNumber!.Contains(filter.SearchQuery));
            }

            // Grade filter
            if (filter.GradeLevelId.HasValue)
            {
                query = query.Where(x => x.GradeLevelId == filter.GradeLevelId);
            }

            // Subject filter
            if (filter.SubjectId.HasValue)
            {
                query = query.Where(x =>
                    x.Enrollments.Any(e => e.IsActive && e.Group.Course.SubjectId == filter.SubjectId));
            }

            // Active filter
            if (!filter.IncludeInactive)
            {
                query = query.Where(x => x.User.IsActive);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Paginate
            var students = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Map to DTOs with computed fields
            var items = students.Select(s => MapToListItemDto(s)).ToList();

            // Post-filter by payment status (computed in memory)
            if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
            {
                items = items.Where(i =>
                    i.PaymentStatus.Equals(filter.PaymentStatus, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return new PagedResult<StudentListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        // =========================================
        // GetStudentProfileAsync
        // =========================================

        public async Task<StudentProfileDto> GetStudentProfileAsync(int studentProfileId)
        {
            var student = await _db.StudentProfiles
                .Include(x => x.User)
                .Include(x => x.GradeLevel)
                .Include(x => x.Enrollments)
                    .ThenInclude(e => e.Group)
                        .ThenInclude(g => g.Course)
                            .ThenInclude(c => c.Subject)
                .Include(x => x.Enrollments)
                    .ThenInclude(e => e.Group)
                        .ThenInclude(g => g.InstructorProfile)
                            .ThenInclude(ip => ip.User)
                .Include(x => x.CoursePayments)
                    .ThenInclude(cp => cp.Course)
                .Include(x => x.Attendances)
                    .ThenInclude(a => a.Session)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Course)
                                .ThenInclude(c => c.Subject)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == studentProfileId)
                ?? throw new KeyNotFoundException($"Student profile {studentProfileId} not found.");

            var listItem = MapToListItemDto(student);

            return new StudentProfileDto
            {
                StudentProfileId = listItem.StudentProfileId,
                UserId = listItem.UserId,
                FullName = listItem.FullName,
                PhoneNumber = listItem.PhoneNumber,
                ParentPhone = listItem.ParentPhone,
                GradeLevelName = listItem.GradeLevelName,
                AttendanceRatePercent = listItem.AttendanceRatePercent,
                PaymentStatus = listItem.PaymentStatus,
                IsActive = listItem.IsActive,
                ImagePath = listItem.ImagePath,
                CreatedAt = listItem.CreatedAt,
                Email = student.User.Email ?? string.Empty,

                Enrollments = student.Enrollments.Select(e => new EnrollmentDto
                {
                    EnrollmentId = e.Id,
                    GroupId = e.GroupId,
                    GroupName = e.Group.Name,
                    CourseName = e.Group.Course.Name,
                    SubjectName = e.Group.Course.Subject.Name,
                    InstructorName = e.Group.InstructorProfile?.User?.FullName ?? "N/A",
                    IsActive = e.IsActive,
                    EnrollmentDate = e.EnrollmentDate
                }).ToList(),

                CoursePayments = student.CoursePayments.Select(cp => new StudentCoursePaymentSummaryDto
                {
                    StudentCoursePaymentId = cp.Id,
                    CourseName = cp.Course.Name,
                    RequiredAmount = cp.RequiredAmount,
                    PaidAmount = cp.PaidAmount,
                    RemainingAmount = cp.RemainingAmount,
                    IsPaid = cp.IsPaid
                }).ToList(),

                RecentAttendances = student.Attendances
                    .OrderByDescending(a => a.Session.SessionDate)
                    .Take(10)
                    .Select(a => new StudentAttendanceDto
                    {
                        AttendanceId = a.Id,
                        SessionDate = a.Session.SessionDate,
                        SubjectName = a.Session.Group.Course.Subject.Name,
                        GroupName = a.Session.Group.Name,
                        ScanTime = a.ScanTime,
                        IsPresent = a.IsPresent,
                        IsLate = a.IsLate
                    }).ToList(),

                UpcomingSessions = new() // Phase 3 populates this
            };
        }

        // =========================================
        // UpdateStudentAsync
        // =========================================

        public async Task UpdateStudentAsync(int id, UpdateStudentDto dto, string adminId)
        {
            var profile = await _db.StudentProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Student profile {id} not found.");

            var oldValues = JsonSerializer.Serialize(new
            {
                profile.User.FullName,
                profile.User.PhoneNumber,
                profile.ParentPhone,
                profile.GradeLevelId
            });

            // Update user fields
            profile.User.FullName = dto.FullName;
            profile.User.PhoneNumber = dto.PhoneNumber;

            // Update profile fields
            profile.ParentPhone = dto.ParentPhone;
            profile.GradeLevelId = dto.GradeLevelId;

            // Handle photo upload
            if (dto.Photo != null)
            {
                // Delete old photo if exists
                if (!string.IsNullOrEmpty(profile.User.ImagePath))
                {
                    _fileUpload.DeleteFile(profile.User.ImagePath);
                }
                profile.User.ImagePath = await _fileUpload.UploadPhotoAsync(dto.Photo, "students");
            }

            await _userManager.UpdateAsync(profile.User);
            await _db.SaveChangesAsync();

            var newValues = JsonSerializer.Serialize(new
            {
                dto.FullName,
                dto.PhoneNumber,
                dto.ParentPhone,
                dto.GradeLevelId
            });

            await _audit.LogAsync(adminId, "StudentUpdated", "StudentProfile", id, oldValues, newValues);
        }

        // =========================================
        // SoftDeleteAsync
        // =========================================

        public async Task SoftDeleteAsync(int id, string adminId)
        {
            var profile = await _db.StudentProfiles
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Student profile {id} not found.");

            profile.IsDeleted = true;
            profile.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "StudentSoftDeleted", "StudentProfile", id, null, null);
        }

        // =========================================
        // ToggleActiveAsync
        // =========================================

        public async Task ToggleActiveAsync(int id, string adminId)
        {
            var profile = await _db.StudentProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Student profile {id} not found.");

            profile.User.IsActive = !profile.User.IsActive;
            await _userManager.UpdateAsync(profile.User);

            await _audit.LogAsync(
                adminId,
                "StudentActiveToggled",
                "StudentProfile",
                id,
                null,
                JsonSerializer.Serialize(new { IsActive = profile.User.IsActive }));
        }

        // =========================================
        // TransferStudentAsync
        // =========================================

        public async Task TransferStudentAsync(int studentProfileId, int fromGroupId, int toGroupId, string adminId)
        {
            // 1. Find active enrollment in fromGroup
            var enrollment = await _db.Enrollments
                .Include(e => e.Group)
                .FirstOrDefaultAsync(e =>
                    e.StudentProfileId == studentProfileId &&
                    e.GroupId == fromGroupId &&
                    e.IsActive)
                ?? throw new InvalidOperationException(
                    $"No active enrollment found for student {studentProfileId} in group {fromGroupId}.");

            var targetGroup = await _db.Groups
                .Include(g => g.Course)
                .FirstOrDefaultAsync(g => g.Id == toGroupId);
            if (targetGroup == null) throw new InvalidOperationException("Target group not found or is deleted.");
            if (!targetGroup.IsActive) throw new InvalidOperationException("Target group is inactive.");

            // 2. Check no existing active enrollment in toGroup
            var existingActive = await _db.Enrollments
                .AnyAsync(e =>
                    e.StudentProfileId == studentProfileId &&
                    e.GroupId == toGroupId &&
                    e.IsActive);

            if (existingActive)
            {
                throw new InvalidOperationException(
                    $"Student {studentProfileId} already has an active enrollment in group {toGroupId}.");
            }

            // GAP-004: Financial Reconciliation
            var oldCourseId = enrollment.Group.CourseId;
            var newCourseId = targetGroup.CourseId;
            if (oldCourseId != newCourseId)
            {
                var oldPayment = await _db.StudentCoursePayments
                    .FirstOrDefaultAsync(p => p.StudentProfileId == studentProfileId && p.CourseId == oldCourseId && !p.IsDeleted);

                if (oldPayment != null)
                {
                    // Check if payment already exists for the new course
                    var newPayment = await _db.StudentCoursePayments
                        .FirstOrDefaultAsync(p => p.StudentProfileId == studentProfileId && p.CourseId == newCourseId && !p.IsDeleted);

                    if (newPayment == null)
                    {
                        var newPrice = targetGroup.Course.Price;
                        var paidAmount = oldPayment.PaidAmount;
                        
                        _db.StudentCoursePayments.Add(new StudentCoursePayment
                        {
                            StudentProfileId = studentProfileId,
                            CourseId = newCourseId,
                            RequiredAmount = newPrice,
                            PaidAmount = paidAmount,
                            RemainingAmount = Math.Max(0, newPrice - paidAmount),
                            IsPaid = paidAmount >= newPrice,
                            CreatedAt = DateTime.UtcNow
                        });
                        
                        // We logically delete the old payment so it no longer applies to the student's balance, but it stays in history
                        oldPayment.IsDeleted = true;
                        oldPayment.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // 3. Deactivate old enrollment
            enrollment.IsActive = false;
            enrollment.UpdatedAt = DateTime.UtcNow;

            // 4. Create new enrollment
            _db.Enrollments.Add(new Enrollment
            {
                StudentProfileId = studentProfileId,
                GroupId = toGroupId,
                IsActive = true,
                EnrollmentDate = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // 5. Audit
            await _audit.LogAsync(
                adminId,
                "StudentTransferred",
                "Enrollment",
                enrollment.Id,
                JsonSerializer.Serialize(new { FromGroupId = fromGroupId }),
                JsonSerializer.Serialize(new { ToGroupId = toGroupId }));
        }

        // =========================================
        // AddToGroupAsync
        // =========================================

        public async Task AddToGroupAsync(int studentProfileId, int groupId, string adminId)
        {
            var targetGroup = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (targetGroup == null) throw new InvalidOperationException("Target group not found or is deleted.");
            if (!targetGroup.IsActive) throw new InvalidOperationException("Target group is inactive.");

            // Check if enrollment already exists (active or not)
            var existing = await _db.Enrollments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e =>
                    e.StudentProfileId == studentProfileId &&
                    e.GroupId == groupId);

            if (existing != null)
            {
                if (existing.IsActive)
                {
                    throw new InvalidOperationException("Student is already enrolled in this group.");
                }

                // Reactivate existing enrollment
                existing.IsActive = true;
                existing.EnrollmentDate = DateTime.UtcNow;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.Enrollments.Add(new Enrollment
                {
                    StudentProfileId = studentProfileId,
                    GroupId = groupId,
                    IsActive = true,
                    EnrollmentDate = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "StudentAddedToGroup", "Enrollment", studentProfileId,
                null, JsonSerializer.Serialize(new { GroupId = groupId }));
        }

        // =========================================
        // RemoveFromGroupAsync
        // =========================================

        public async Task RemoveFromGroupAsync(int studentProfileId, int groupId, string adminId)
        {
            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e =>
                    e.StudentProfileId == studentProfileId &&
                    e.GroupId == groupId &&
                    e.IsActive)
                ?? throw new InvalidOperationException(
                    $"No active enrollment found for student {studentProfileId} in group {groupId}.");

            enrollment.IsActive = false;
            enrollment.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "StudentRemovedFromGroup", "Enrollment", studentProfileId,
                null, JsonSerializer.Serialize(new { GroupId = groupId }));
        }

        // =========================================
        // SearchStudentsAsync
        // =========================================

        public async Task<List<StudentListItemDto>> SearchStudentsAsync(string query, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<StudentListItemDto>();

            var students = await _db.StudentProfiles
                .Include(x => x.User)
                .Include(x => x.GradeLevel)
                .Include(x => x.CoursePayments)
                .AsNoTracking()
                .Where(x =>
                    x.User.FullName.Contains(query) ||
                    x.User.PhoneNumber!.Contains(query))
                .Where(x => x.User.IsActive)
                .Take(maxResults)
                .ToListAsync();

            return students.Select(s => MapToListItemDto(s)).ToList();
        }

        // =========================================
        // Private Helpers
        // =========================================

        private static StudentListItemDto MapToListItemDto(StudentProfile s)
        {
            // Compute attendance rate
            var totalAttendances = s.Attendances?.Count ?? 0;
            var presentCount = s.Attendances?.Count(a => a.IsPresent) ?? 0;
            var attendanceRate = totalAttendances > 0
                ? Math.Round((decimal)presentCount / totalAttendances * 100, 1)
                : 0m;

            // Compute payment status
            var payments = s.CoursePayments?.ToList() ?? new List<StudentCoursePayment>();
            string paymentStatus;
            if (!payments.Any())
            {
                paymentStatus = "Unpaid";
            }
            else if (payments.All(p => p.IsPaid))
            {
                paymentStatus = "Paid";
            }
            else if (payments.Any(p => p.PaidAmount > 0))
            {
                paymentStatus = "Partial";
            }
            else
            {
                paymentStatus = "Unpaid";
            }

            return new StudentListItemDto
            {
                StudentProfileId = s.Id,
                UserId = s.UserId,
                FullName = s.User.FullName,
                PhoneNumber = s.User.PhoneNumber,
                ParentPhone = s.ParentPhone,
                GradeLevelName = s.GradeLevel?.Name ?? "N/A",
                AttendanceRatePercent = attendanceRate,
                PaymentStatus = paymentStatus,
                IsActive = s.User.IsActive,
                ImagePath = s.User.ImagePath,
                CreatedAt = s.CreatedAt
            };
        }
    }
}
