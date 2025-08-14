using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationsController(
            INotificationService notificationService,
            ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto createNotificationDto)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(createNotificationDto);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating notification: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized("User not authenticated");
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(currentUserId, page, pageSize);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting notifications: {ex.Message}");
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized("User not authenticated");
                }

                var count = await _notificationService.GetUnreadNotificationCountAsync(currentUserId);
                return Ok(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting unread notification count: {ex.Message}");
            }
        }

        [HttpPut("{notificationId}/mark-read")]
        public async Task<IActionResult> MarkNotificationAsRead(string notificationId)
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized("User not authenticated");
                }

                var success = await _notificationService.MarkNotificationAsReadAsync(notificationId, currentUserId);
                if (!success)
                {
                    return NotFound("Notification not found or doesn't belong to the user");
                }

                return Ok(new { Message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error marking notification as read: {ex.Message}");
            }
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized("User not authenticated");
                }

                var success = await _notificationService.MarkAllNotificationsAsReadAsync(currentUserId);
                return Ok(new { Message = success ? "All notifications marked as read" : "No unread notifications found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error marking all notifications as read: {ex.Message}");
            }
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(string notificationId)
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized("User not authenticated");
                }

                var success = await _notificationService.DeleteNotificationAsync(notificationId, currentUserId);
                if (!success)
                {
                    return NotFound("Notification not found or doesn't belong to the user");
                }

                return Ok(new { Message = "Notification deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting notification: {ex.Message}");
            }
        }
    }
}
