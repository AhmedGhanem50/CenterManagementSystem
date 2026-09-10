using CenterManagement.Application.DTOs.Common;
using CenterManagement.Application.DTOs.Payment;
using CenterManagement.Domain.Entities;

namespace CenterManagement.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<StudentCoursePayment> CreateCoursePaymentAsync(
            int studentProfileId, int courseId, string adminId);

        Task<PaymentTransaction> RecordPaymentAsync(
            RecordPaymentDto dto, string adminId);

        Task VoidTransactionAsync(int transactionId, string reason, string adminId);

        Task<SessionPayment> CreateSessionPaymentAsync(
            CreateSessionPaymentDto dto, string adminId);

        Task<StudentFinancialSummaryDto> GetStudentFinancialSummaryAsync(int studentProfileId);

        Task<PagedResult<TransactionListItemDto>> GetTransactionListAsync(TransactionFilter filter);

        Task<PaymentKpiDto> GetPaymentKpisAsync();

        Task<List<OutstandingStudentDto>> GetOutstandingStudentsAsync();

        Task<byte[]> ExportTransactionsCsvAsync(TransactionFilter filter);
    }
}
