using CenterManagement.Application.DTOs.Session;

namespace CenterManagement.Application.Interfaces
{
    public interface ISessionService
    {
        Task<int> CreateSessionAsync(CreateSessionDto dto, string adminId);
        Task<SessionDetailDto> GetSessionDetailAsync(int sessionId);
        Task<List<SessionListItemDto>> GetSessionsByGroupAsync(int groupId, DateTime? from, DateTime? to);
        Task<List<SessionListItemDto>> GetSessionsByDateAsync(DateTime date);
        Task<List<SessionListItemDto>> GetSessionsByDateRangeAsync(DateTime from, DateTime to);
        Task CancelSessionAsync(int sessionId, string cancelReason, string performedByUserId);
        Task UncancelSessionAsync(int sessionId, string adminId);
        Task AssignSubstituteAsync(int sessionId, int substituteInstructorProfileId, string adminId);
        Task<int?> GetInstructorProfileIdByUserIdAsync(string userId);
    }
}
