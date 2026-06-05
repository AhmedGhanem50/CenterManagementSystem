using CenterManagement.Application.DTOs.Attendance;
using CenterManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IQrService _qrService;

        public AttendanceController(
            IAttendanceService attendanceService,
            IQrService qrService)
        {
            _attendanceService = attendanceService;
            _qrService = qrService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // =========================================
        // POST /Attendance/Scan
        // =========================================

        [HttpPost]
        public async Task<IActionResult> Scan([FromBody] ScanRequest req)
        {
            try
            {
                var result = await _attendanceService.ProcessScanAsync(req.QrCode, DateTime.UtcNow);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // POST /Attendance/MarkManual
        // =========================================

        [HttpPost]
        public async Task<IActionResult> MarkManual([FromBody] ManualMarkDto dto)
        {
            try
            {
                // Admin only inline check
                if (!User.IsInRole("Admin"))
                {
                    return Json(new { success = false, error = "Only admins can mark attendance manually." });
                }

                await _attendanceService.MarkManuallyAsync(dto, GetUserId());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // GET /Attendance/SessionSummary/{sessionId}
        // =========================================

        [HttpGet]
        public async Task<IActionResult> SessionSummary(int sessionId)
        {
            try
            {
                var summary = await _attendanceService.GetSessionSummaryAsync(sessionId);
                return Json(summary);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // GET /Attendance/SessionList/{sessionId}
        // =========================================

        [HttpGet]
        public async Task<IActionResult> SessionList(int sessionId, int page = 1, string? q = null)
        {
            try
            {
                var pagedList = await _attendanceService.GetSessionAttendanceListAsync(
                    sessionId, page, 20, q);
                return Json(pagedList);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // GET /Attendance/StudentHistory/{studentProfileId}
        // =========================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StudentHistory(int studentProfileId, int? groupId, DateTime? from, DateTime? to)
        {
            try
            {
                var list = await _attendanceService.GetStudentAttendanceHistoryAsync(
                    studentProfileId, groupId, from, to);
                return Json(list);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // GET /Attendance/GetQr/{userId}
        // =========================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetQr(string userId)
        {
            try
            {
                var qrContent = _qrService.GenerateStudentQrCode(userId);
                var bytes = _qrService.GenerateQrCodeImage(qrContent);
                return File(bytes, "image/png", $"qr_{userId}.png");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }

    // Inline request model
    public record ScanRequest(string QrCode);
}
