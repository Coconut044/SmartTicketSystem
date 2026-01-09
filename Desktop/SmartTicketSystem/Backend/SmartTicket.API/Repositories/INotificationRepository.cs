using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Repositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}