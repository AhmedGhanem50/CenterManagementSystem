using CenterManagement.Infrastructure.Persistence;
using CenterManagement.Web.ViewModels.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class SessionController : Controller
    {
        private readonly CenterManagementDbContext _db;

        public SessionController(CenterManagementDbContext db)
        {
            _db = db;
        }

        // =========================================
        // GET /Session/Detail/{id}
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var session = await _db.Sessions
                .Include(s => s.Group)
                    .ThenInclude(g => g.Course)
                        .ThenInclude(c => c.Subject)
                .Include(s => s.Group)
                    .ThenInclude(g => g.Course)
                        .ThenInclude(c => c.GradeLevel)
                .Include(s => s.Group)
                    .ThenInclude(g => g.InstructorProfile)
                        .ThenInclude(ip => ip.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();

            var viewModel = new SessionDetailViewModel
            {
                SessionId = session.Id,
                GroupName = session.Group.Name,
                CourseName = session.Group.Course.Name,
                SubjectName = session.Group.Course.Subject.Name,
                InstructorName = session.Group.InstructorProfile.User.FullName,
                GradeLevelName = session.Group.Course.GradeLevel.Name,
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                IsCanceled = session.IsCanceled,
                CancelReason = session.CancelReason
            };

            return View(viewModel);
        }
    }
}
