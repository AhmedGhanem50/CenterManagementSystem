using CenterManagement.Application.DTOs.Common;
using CenterManagement.Application.DTOs.Student;
using CenterManagement.Application.Interfaces;
using CenterManagement.Infrastructure.Persistence;
using CenterManagement.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly CenterManagementDbContext _db;
        private readonly ISystemSettingService _settingService;

        public StudentController(
            IStudentService studentService,
            CenterManagementDbContext db,
            ISystemSettingService settingService)
        {
            _studentService = studentService;
            _db = db;
            _settingService = settingService;
        }

        private string GetAdminId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // =========================================
        // GET /Student
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index(StudentListFilter filter)
        {
            var result = await _studentService.GetStudentListAsync(filter);

            var gradeLevels = await _db.GradeLevels
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .ToListAsync();

            var subjects = await _db.Subjects
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

            var viewModel = new StudentListViewModel
            {
                Students = result.Items,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                SearchQuery = filter.SearchQuery,
                GradeLevelId = filter.GradeLevelId,
                SubjectId = filter.SubjectId,
                PaymentStatusFilter = filter.PaymentStatus,
                GradeLevelSelectList = new SelectList(gradeLevels, "Id", "Name", filter.GradeLevelId),
                SubjectSelectList = new SelectList(subjects, "Id", "Name", filter.SubjectId)
            };

            return View(viewModel);
        }

        // =========================================
        // GET /Student/Create
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateStudentViewModel
            {
                GradeLevelSelectList = await GetGradeLevelSelectList(),
                GroupSelectList = await GetGroupSelectList()
            };
            return View(model);
        }

        // =========================================
        // POST /Student/Create
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreateStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.GradeLevelSelectList = await GetGradeLevelSelectList();
                model.GroupSelectList = await GetGroupSelectList();
                return View("Create", model);
            }

            try
            {
                var dto = new CreateStudentDto
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    ParentPhone = model.ParentPhone,
                    GradeLevelId = model.GradeLevelId,
                    GroupIds = model.GroupIds,
                    Photo = model.Photo
                };

                await _studentService.CreateStudentAsync(dto, GetAdminId());
                TempData["Success"] = "Student created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.GradeLevelSelectList = await GetGradeLevelSelectList();
                model.GroupSelectList = await GetGroupSelectList();
                return View("Create", model);
            }
        }

        // =========================================
        // GET /Student/Profile/{id}
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            try
            {
                var profile = await _studentService.GetStudentProfileAsync(id);
                ViewBag.Courses = await GetCourseSelectList();
                return View(profile);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // =========================================
        // GET /Student/Edit/{id}
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var profile = await _studentService.GetStudentProfileAsync(id);

                var model = new UpdateStudentViewModel
                {
                    StudentProfileId = id,
                    FullName = profile.FullName,
                    PhoneNumber = profile.PhoneNumber,
                    ParentPhone = profile.ParentPhone,
                    GradeLevelId = _db.GradeLevels
                        .AsNoTracking()
                        .FirstOrDefault(g => g.Name == profile.GradeLevelName)?.Id ?? 0,
                    CurrentImagePath = profile.ImagePath,
                    GradeLevelSelectList = await GetGradeLevelSelectList()
                };

                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // =========================================
        // POST /Student/Edit/{id}
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, UpdateStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.GradeLevelSelectList = await GetGradeLevelSelectList();
                return View("Edit", model);
            }

            try
            {
                var dto = new UpdateStudentDto
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    ParentPhone = model.ParentPhone,
                    GradeLevelId = model.GradeLevelId,
                    Photo = model.Photo
                };

                await _studentService.UpdateStudentAsync(id, dto, GetAdminId());
                TempData["Success"] = "Student updated successfully.";
                return RedirectToAction(nameof(Profile), new { id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.GradeLevelSelectList = await GetGradeLevelSelectList();
                return View("Edit", model);
            }
        }

        // =========================================
        // POST /Student/Delete/{id}
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _studentService.SoftDeleteAsync(id, GetAdminId());
                TempData["Success"] = "Student archived successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Student not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // POST /Student/ToggleActive/{id}
        // =========================================

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                await _studentService.ToggleActiveAsync(id, GetAdminId());
                var profile = await _studentService.GetStudentProfileAsync(id);
                return Json(new { success = true, isActive = profile.IsActive });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // GET /Student/Transfer/{id}
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Transfer(int id)
        {
            try
            {
                var profile = await _studentService.GetStudentProfileAsync(id);
                var activeEnrollment = profile.Enrollments.FirstOrDefault(e => e.IsActive);

                if (activeEnrollment == null)
                {
                    TempData["Error"] = "No active enrollment found for this student.";
                    return RedirectToAction(nameof(Profile), new { id });
                }

                var groups = await _db.Groups
                    .Include(g => g.Course)
                    .Where(g => g.IsActive && g.Id != activeEnrollment.GroupId)
                    .AsNoTracking()
                    .ToListAsync();

                var model = new TransferStudentViewModel
                {
                    StudentProfileId = id,
                    StudentName = profile.FullName,
                    FromGroupId = activeEnrollment.GroupId,
                    FromGroupName = activeEnrollment.GroupName,
                    AvailableGroupsSelectList = new SelectList(
                        groups.Select(g => new { g.Id, Display = $"{g.Name} — {g.Course.Name}" }),
                        "Id", "Display")
                };

                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // =========================================
        // POST /Student/Transfer
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferPost(TransferStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var groups = await _db.Groups
                    .Include(g => g.Course)
                    .Where(g => g.IsActive && g.Id != model.FromGroupId)
                    .AsNoTracking()
                    .ToListAsync();

                model.AvailableGroupsSelectList = new SelectList(
                    groups.Select(g => new { g.Id, Display = $"{g.Name} — {g.Course.Name}" }),
                    "Id", "Display");

                return View("Transfer", model);
            }

            try
            {
                await _studentService.TransferStudentAsync(
                    model.StudentProfileId,
                    model.FromGroupId,
                    model.ToGroupId,
                    GetAdminId());

                TempData["Success"] = "Student transferred successfully.";
                return RedirectToAction(nameof(Profile), new { id = model.StudentProfileId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                var groups = await _db.Groups
                    .Include(g => g.Course)
                    .Where(g => g.IsActive && g.Id != model.FromGroupId)
                    .AsNoTracking()
                    .ToListAsync();

                model.AvailableGroupsSelectList = new SelectList(
                    groups.Select(g => new { g.Id, Display = $"{g.Name} — {g.Course.Name}" }),
                    "Id", "Display");

                return View("Transfer", model);
            }
        }

        // =========================================
        // POST /Student/AddToGroup
        // =========================================

        [HttpPost]
        public async Task<IActionResult> AddToGroup([FromBody] AddRemoveGroupRequest request)
        {
            try
            {
                await _studentService.AddToGroupAsync(request.StudentProfileId, request.GroupId, GetAdminId());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // POST /Student/RemoveFromGroup
        // =========================================

        [HttpPost]
        public async Task<IActionResult> RemoveFromGroup([FromBody] AddRemoveGroupRequest request)
        {
            try
            {
                await _studentService.RemoveFromGroupAsync(request.StudentProfileId, request.GroupId, GetAdminId());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // GET /Student/Search
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            var results = await _studentService.SearchStudentsAsync(q);
            return Json(results);
        }

        // =========================================
        // GET /Student/Card/{id}
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Card(int id)
        {
            try
            {
                var profile = await _studentService.GetStudentProfileAsync(id);
                var logoUrl = await _settingService.GetSettingAsync("CenterLogoUrl");
                
                ViewBag.CenterLogoUrl = string.IsNullOrEmpty(logoUrl) ? "/images/default-logo.png" : logoUrl;
                
                return View(profile);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // =========================================
        // Private Helpers
        // =========================================

        private async Task<SelectList> GetGradeLevelSelectList()
        {
            var gradeLevels = await _db.GradeLevels
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .ToListAsync();

            return new SelectList(gradeLevels, "Id", "Name");
        }

        private async Task<SelectList> GetGroupSelectList()
        {
            var groups = await _db.Groups
                .Include(g => g.Course)
                .Where(g => g.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return new SelectList(
                groups.Select(g => new { g.Id, Display = $"{g.Name} — {g.Course.Name}" }),
                "Id", "Display");
        }

        private async Task<SelectList> GetCourseSelectList()
        {
            var courses = await _db.Courses
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new SelectList(courses, "Id", "Name");
        }
    }

    // Simple request body for AddToGroup / RemoveFromGroup
    public class AddRemoveGroupRequest
    {
        public int StudentProfileId { get; set; }
        public int GroupId { get; set; }
    }
}
