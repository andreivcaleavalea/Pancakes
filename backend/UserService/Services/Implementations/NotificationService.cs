using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IMapper mapper,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
        {
            _logger.LogInformation("Creating notification for user {UserId} of type {Type}", 
                createNotificationDto.UserId, createNotificationDto.Type);

            var notification = _mapper.Map<Notification>(createNotificationDto);
            
            var createdNotification = await _notificationRepository.CreateAsync(notification);
            
            _logger.LogInformation("Notification {NotificationId} created successfully", createdNotification.Id);
            
            return _mapper.Map<NotificationDto>(createdNotification);
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            _logger.LogInformation("Getting notifications for user {UserId}, page {Page}, size {PageSize}", 
                userId, page, pageSize);

            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize);
            
            return _mapper.Map<List<NotificationDto>>(notifications);
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            _logger.LogInformation("Getting unread notification count for user {UserId}", userId);

            return await _notificationRepository.GetUnreadNotificationCountAsync(userId);
        }

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", 
                notificationId, userId);

            return await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);

            var count = await _notificationRepository.MarkAllAsReadAsync(userId);
            
            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", count, userId);
            
            return count > 0;
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId, string userId)
        {
            _logger.LogInformation("Deleting notification {NotificationId} for user {UserId}", 
                notificationId, userId);

            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            
            if (notification == null || notification.UserId != userId)
            {
                _logger.LogWarning("Notification {NotificationId} not found or doesn't belong to user {UserId}", 
                    notificationId, userId);
                return false;
            }

            await _notificationRepository.DeleteAsync(notificationId);
            
            _logger.LogInformation("Notification {NotificationId} deleted successfully", notificationId);
            
            return true;
        }
    }
}

