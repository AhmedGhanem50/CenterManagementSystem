using CenterManagement.Application.DTOs.Payment;
using CenterManagement.Application.Interfaces;
using CenterManagement.Infrastructure.Persistence;
using CenterManagement.Web.ViewModels.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly CenterManagementDbContext _db;

        public PaymentController(
            IPaymentService paymentService,
            CenterManagementDbContext db)
        {
            _paymentService = paymentService;
            _db = db;
        }

        private string GetAdminId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // =========================================
        // GET /Payment
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var kpis = await _paymentService.GetPaymentKpisAsync();
            var viewModel = new PaymentIndexViewModel
            {
                Kpis = kpis
            };
            return View(viewModel);
        }

        // =========================================
        // GET /Payment/KPIs
        // =========================================

        [HttpGet]
        public async Task<IActionResult> KPIs()
        {
            var kpis = await _paymentService.GetPaymentKpisAsync();
            return Json(kpis);
        }

        // =========================================
        // GET /Payment/Transactions
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Transactions([FromQuery] TransactionFilter filter)
        {
            var result = await _paymentService.GetTransactionListAsync(filter);
            return Json(result);
        }

        // =========================================
        // POST /Payment/RecordCourse
        // =========================================

        [HttpPost]
        public async Task<IActionResult> RecordCourse([FromBody] RecordPaymentDto dto)
        {
            try
            {
                var adminId = GetAdminId();
                var tx = await _paymentService.RecordPaymentAsync(dto, adminId);
                var scp = await _db.StudentCoursePayments.FindAsync(dto.StudentCoursePaymentId);
                return Json(new
                {
                    success = true,
                    newPaidAmount = scp!.PaidAmount,
                    newRemainingAmount = scp.RemainingAmount,
                    isPaid = scp.IsPaid
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // POST /Payment/CreateCourse
        // =========================================

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCoursePaymentRequest request)
        {
            try
            {
                var adminId = GetAdminId();
                var scp = await _paymentService.CreateCoursePaymentAsync(
                    request.StudentProfileId, request.CourseId, adminId);
                return Json(new
                {
                    success = true,
                    studentCoursePaymentId = scp.Id,
                    requiredAmount = scp.RequiredAmount,
                    paidAmount = scp.PaidAmount,
                    remainingAmount = scp.RemainingAmount,
                    isPaid = scp.IsPaid
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // POST /Payment/RecordSession
        // =========================================

        [HttpPost]
        public async Task<IActionResult> RecordSession([FromBody] CreateSessionPaymentDto dto)
        {
            try
            {
                var adminId = GetAdminId();
                var sp = await _paymentService.CreateSessionPaymentAsync(dto, adminId);
                return Json(new
                {
                    success = true,
                    sessionPaymentId = sp.Id,
                    amount = sp.Amount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // POST /Payment/VoidTransaction
        // =========================================

        [HttpPost]
        public async Task<IActionResult> VoidTransaction(int transactionId, [FromBody] VoidTransactionRequest request)
        {
            try
            {
                var adminId = GetAdminId();
                await _paymentService.VoidTransactionAsync(transactionId, request.Reason, adminId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // =========================================
        // GET /Payment/StudentSummary/{id}
        // =========================================

        [HttpGet]
        public async Task<IActionResult> StudentSummary(int id)
        {
            try
            {
                var summary = await _paymentService.GetStudentFinancialSummaryAsync(id);
                return Json(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // =========================================
        // GET /Payment/Outstanding
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Outstanding()
        {
            var students = await _paymentService.GetOutstandingStudentsAsync();
            return Json(students);
        }

        // =========================================
        // GET /Payment/Export
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] TransactionFilter filter)
        {
            var bytes = await _paymentService.ExportTransactionsCsvAsync(
                filter ?? new TransactionFilter());
            return File(bytes, "text/csv; charset=utf-8", "transactions.csv");
        }
    }

    // Simple request body for CreateCourse
    public class CreateCoursePaymentRequest
    {
        public int StudentProfileId { get; set; }
        public int CourseId { get; set; }
    }

    public class VoidTransactionRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
