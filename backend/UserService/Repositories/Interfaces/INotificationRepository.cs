using UserService.Models.Entities;

namespace UserService.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(string id);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int page, int pageSize);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification> UpdateAsync(Notification notification);
        Task DeleteAsync(string id);
        Task<bool> MarkAsReadAsync(string notificationId, string userId);
        Task<int> MarkAllAsReadAsync(string userId);
    }
}

