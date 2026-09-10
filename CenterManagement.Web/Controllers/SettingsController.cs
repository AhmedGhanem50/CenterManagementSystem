using CenterManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ISystemSettingService _settingService;
        private readonly IFileUploadService _fileUploadService;

        public SettingsController(ISystemSettingService settingService, IFileUploadService fileUploadService)
        {
            _settingService = settingService;
            _fileUploadService = fileUploadService;
        }

        private string GetAdminId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logoUrl = await _settingService.GetSettingAsync("CenterLogoUrl");
            ViewBag.CenterLogoUrl = logoUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadLogo(IFormFile logoFile)
        {
            if (logoFile != null && logoFile.Length > 0)
            {
                var adminId = GetAdminId();
                var path = await _fileUploadService.UploadFileAsync(logoFile, "settings");
                await _settingService.SetSettingAsync("CenterLogoUrl", path, adminId);
                TempData["Success"] = "Logo updated successfully.";
            }
            else
            {
                TempData["Error"] = "Please select a valid image file.";
            }

            return RedirectToAction("Index");
        }
    }
}
