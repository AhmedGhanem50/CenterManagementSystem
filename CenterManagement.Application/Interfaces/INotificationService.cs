using CenterManagement.Application.DTOs.Notification;

namespace CenterManagement.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendToUserAsync(string userId, string title, string message);
        Task SendToGroupAsync(int groupId, string title, string message);
        Task<int> GetUnreadCountAsync(string userId);
        Task<List<NotificationDto>> GetNotificationsAsync(string userId, int page, int pageSize);
        Task MarkReadAsync(int notificationId, string userId);
        Task MarkAllReadAsync(string userId);
        Task RetryFailedNotificationsAsync();
    }
}
