using System.Security.Claims;
using CenterManagement.Application.DTOs.Attendance;
using CenterManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CenterManagement.Web.Controllers;

[Authorize(Roles = "Admin,Instructor")]
public class AttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;
    private readonly IQrService _qrService;

    public AttendanceController(IAttendanceService attendanceService, IQrService qrService)
    {
        _attendanceService = attendanceService;
        _qrService = qrService;
    }

    [HttpGet]
    public IActionResult OfflineScan()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Scan([FromBody] ScanRequest req)
    {
        try
        {
            var result = await _attendanceService.ProcessScanAsync(req.QrCode, DateTime.Now);
            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, errorMessage = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BatchSync([FromBody] List<OfflineScanDto> scans)
    {
        try
        {
            var results = new List<ScanResultDto>();
            foreach (var scan in scans)
            {
                var result = await _attendanceService.ProcessScanAsync(scan.QrCode, scan.ScanTime);
                results.Add(result);
            }
            return Json(new { success = true, results });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, errorMessage = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkManual([FromBody] ManualMarkDto dto)
    {
        try
        {
            if (!User.IsInRole("Admin")) 
            {
                return Json(new { success = false, error = "Only admins can mark attendance manually." });
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
            await _attendanceService.MarkManuallyAsync(dto, adminId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("Attendance/SessionSummary/{sessionId}")]
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

    [HttpGet("Attendance/SessionList/{sessionId}")]
    public async Task<IActionResult> SessionList(int sessionId, int page = 1, string? q = null)
    {
        try
        {
            var pagedList = await _attendanceService.GetSessionAttendanceListAsync(sessionId, page, 50, q);
            return Json(pagedList);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("Attendance/StudentHistory/{studentProfileId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StudentHistory(int studentProfileId, int? groupId, DateTime? from, DateTime? to)
    {
        try
        {
            var list = await _attendanceService.GetStudentAttendanceHistoryAsync(studentProfileId, groupId, from, to);
            return Json(list);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("Attendance/GetQr/{userId}")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetQr(string userId)
    {
        var content = _qrService.GenerateStudentQrCode(userId);
        var bytes = _qrService.GenerateQrCodeImage(content);
        return File(bytes, "image/png", $"qr_{userId}.png");
    }
}

public record ScanRequest(string QrCode);

public class OfflineScanDto
{
    public string QrCode { get; set; } = string.Empty;
    public DateTime ScanTime { get; set; }
}
