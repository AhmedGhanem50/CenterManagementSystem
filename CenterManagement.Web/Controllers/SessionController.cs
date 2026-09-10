using CenterManagement.Application.DTOs.Session;
using CenterManagement.Application.Interfaces;
using CenterManagement.Web.ViewModels.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IGroupService _groupService;

        public SessionController(ISessionService sessionService, IGroupService groupService)
        {
            _sessionService = sessionService;
            _groupService = groupService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: /Session
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date, int? groupId)
        {
            List<SessionListItemDto> sessions;
            if (date.HasValue)
            {
                sessions = await _sessionService.GetSessionsByDateAsync(date.Value);
            }
            else
            {
                // Default to showing sessions from a week ago to a month ahead if no filters
                sessions = await _sessionService.GetSessionsByDateRangeAsync(DateTime.Now.AddDays(-7), DateTime.Now.AddDays(30));
            }

            if (groupId.HasValue)
            {
                var groupDetail = await _groupService.GetGroupDetailAsync(groupId.Value);
                sessions = sessions.Where(s => s.GroupName == groupDetail.Name).ToList();
            }

            // If Instructor, filter to their sessions only
            if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
            {
                var instructorProfileId = await _sessionService.GetInstructorProfileIdByUserIdAsync(GetUserId());
                if (instructorProfileId != null)
                {
                    // This is a simplified filter since SessionListItemDto has InstructorName, but we could also filter securely via service
                    // Since it's for display, let's filter via Group's InstructorProfileId if we could, 
                    // but since we only have InstructorName in DTO, we might want to fetch instructor groups first
                    var groups = await _groupService.GetGroupsByInstructorAsync(instructorProfileId.Value);
                    var groupNames = groups.Select(g => g.Name).ToHashSet();
                    sessions = sessions.Where(s => groupNames.Contains(s.GroupName)).ToList();
                }
            }

            ViewData["CurrentDate"] = date?.ToString("yyyy-MM-dd");
            ViewData["CurrentGroupId"] = groupId;
            
            var allGroups = await _groupService.GetAllGroupsAsync();
            ViewData["GroupSelectList"] = new SelectList(allGroups.Where(g => g.IsActive).OrderBy(g => g.Name), "Id", "Name", groupId);

            return View(sessions);
        }

        // GET: /Session/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!User.IsInRole("Admin")) return Forbid();

            var groups = await _groupService.GetAllGroupsAsync();
            var model = new CreateSessionViewModel
            {
                GroupSelectList = new SelectList(groups.Where(g => g.IsActive).OrderBy(g => g.Name), "Id", "Name")
            };
            return View(model);
        }

        // POST: /Session/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreateSessionViewModel model)
        {
            if (!User.IsInRole("Admin")) return Forbid();

            if (!ModelState.IsValid)
            {
                var groups = await _groupService.GetAllGroupsAsync();
                model.GroupSelectList = new SelectList(groups.Where(g => g.IsActive).OrderBy(g => g.Name), "Id", "Name");
                return View("Create", model);
            }

            try
            {
                var dto = new CreateSessionDto
                {
                    GroupId = model.GroupId,
                    SessionDate = model.SessionDate,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime
                };

                await _sessionService.CreateSessionAsync(dto, GetUserId());
                TempData["Success"] = "Session created successfully.";
                return RedirectToAction(nameof(Index), new { date = model.SessionDate.ToString("yyyy-MM-dd") });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var groups = await _groupService.GetAllGroupsAsync();
                model.GroupSelectList = new SelectList(groups.Where(g => g.IsActive).OrderBy(g => g.Name), "Id", "Name");
                return View("Create", model);
            }
        }

        // GET: /Session/Detail/{id}
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var detail = await _sessionService.GetSessionDetailAsync(id);
                var instructorProfileId = await _sessionService.GetInstructorProfileIdByUserIdAsync(GetUserId());
                
                var canCancel = User.IsInRole("Admin")
                    || (User.IsInRole("Instructor") && instructorProfileId == detail.InstructorProfileId);

                return View(new SessionDetailViewModel 
                { 
                    SessionDetail = detail, 
                    CanCancel = canCancel 
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Session/Cancel/{id}
        [HttpPost]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelSessionRequest request)
        {
            try
            {
                await _sessionService.CancelSessionAsync(id, request.CancelReason, GetUserId());
                return Json(new { success = true, message = "Session canceled successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "You do not own this session's group." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Session/Uncancel/{id}
        [HttpPost]
        public async Task<IActionResult> Uncancel(int id)
        {
            try
            {
                await _sessionService.UncancelSessionAsync(id, GetUserId());
                return Json(new { success = true, message = "Session uncanceled successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "You do not own this session's group." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Session/AssignSubstitute/{id}
        [HttpPost]
        public async Task<IActionResult> AssignSubstitute(int id, [FromBody] AssignSubstituteRequest request)
        {
            try
            {
                await _sessionService.AssignSubstituteAsync(id, request.SubstituteProfileId, GetUserId());
                return Json(new { success = true, message = "Substitute instructor assigned." });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "You do not own this session's group." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class CancelSessionRequest
    {
        public string CancelReason { get; set; } = string.Empty;
    }

    public class AssignSubstituteRequest
    {
        public int SubstituteProfileId { get; set; }
    }
}
