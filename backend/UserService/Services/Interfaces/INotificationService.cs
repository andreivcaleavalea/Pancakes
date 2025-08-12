using UserService.Models.DTOs;

namespace UserService.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createNotificationDto);
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId, string userId);
    }
}

