using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface IFriendshipService
{
    // Original methods (still needed for backward compatibility)
    Task<IEnumerable<FriendDto>> GetUserFriendsAsync(string userId);
    Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(string userId);
    Task<IEnumerable<UserInfoDto>> GetAvailableUsersAsync(string userId);
    Task<FriendshipDto> SendFriendRequestAsync(string senderId, string receiverId);
    Task<FriendshipDto> AcceptFriendRequestAsync(Guid friendshipId, string userId);
    Task<FriendshipDto> RejectFriendRequestAsync(Guid friendshipId, string userId);
    Task RemoveFriendAsync(string userId1, string userId2);
    Task<bool> AreFriendsAsync(string userId1, string userId2);
    
    // New methods that handle business logic internally
    Task<IEnumerable<FriendDto>> GetFriendsAsync(HttpContext httpContext);
    Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(HttpContext httpContext);
    Task<IEnumerable<UserInfoDto>> GetAvailableUsersAsync(HttpContext httpContext);
    Task<FriendshipDto> SendFriendRequestAsync(CreateFriendRequestDto request, HttpContext httpContext);
    Task<FriendshipDto> AcceptFriendRequestAsync(Guid friendshipId, HttpContext httpContext);
    Task<FriendshipDto> RejectFriendRequestAsync(Guid friendshipId, HttpContext httpContext);
    Task RemoveFriendAsync(string friendUserId, HttpContext httpContext);
    Task<bool> CheckFriendshipAsync(string userId, HttpContext httpContext);
} 