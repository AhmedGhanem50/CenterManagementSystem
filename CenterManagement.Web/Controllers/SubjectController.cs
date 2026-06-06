using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubjectController : Controller
    {
        private readonly CenterManagementDbContext _context;

        public SubjectController(CenterManagementDbContext context)
        {
            _context = context;
        }

        // GET: Subject
        public async Task<IActionResult> Index()
        {
            var items = await _context.Subjects.OrderBy(x => x.Name).ToListAsync();
            return View(items);
        }

        // GET: Subject/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Subject/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                // Verify that the subject name does not already exist before inserting
                bool nameExists = await _context.Subjects.AnyAsync(s => s.Name == subject.Name);
                if (nameExists)
                {
                    ModelState.AddModelError("Name", "This subject name already exists. Please choose a different name.");
                    return View(subject);
                }

                _context.Add(subject);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Subject created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        // GET: Subject/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound();

            return View(subject);
        }

        // POST: Subject/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Subject subject)
        {
            if (id != subject.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Verify that the new name is not taken by another subject (excluding the current record itself)
                bool nameExists = await _context.Subjects.AnyAsync(s => s.Name == subject.Name && s.Id != id);
                if (nameExists)
                {
                    ModelState.AddModelError("Name", "This subject name is already in use by another subject.");
                    return View(subject);
                }

                try
                {
                    _context.Update(subject);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Subject updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subject.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        // POST: Subject/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasCourses = await _context.Courses.AnyAsync(c => c.SubjectId == id);
            if (hasCourses)
            {
                TempData["Error"] = "Cannot delete this subject because it is linked to one or more active courses.";
                return RedirectToAction(nameof(Index));
            }

            var subject = await _context.Subjects.FindAsync(id);
            if (subject != null)
            {
                subject.IsDeleted = true;
                _context.Update(subject);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Subject deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.Id == id);
        }
    }
}