using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface IUserServiceClient
{
    Task<UserInfoDto?> GetCurrentUserAsync(string authToken);
    Task<UserInfoDto?> GetUserByIdAsync(string userId);
    Task<UserInfoDto?> GetUserByIdAsync(string userId, string authToken);
    Task<IEnumerable<UserInfoDto>> GetUsersByIdsAsync(IEnumerable<string> userIds, string? authToken = null);
    Task<IEnumerable<UserInfoDto>> GetAllUsersAsync();
    Task<IEnumerable<FriendDto>> GetUserFriendsAsync(string authToken);
    Task<bool> AreFriendsAsync(string userId1, string userId2, string authToken);
    Task<bool> CreateNotificationAsync(string userId, string type, string title, string message, string reason, string source, string? blogTitle = null, string? blogId = null, string? additionalData = null, string? authToken = null);
}
