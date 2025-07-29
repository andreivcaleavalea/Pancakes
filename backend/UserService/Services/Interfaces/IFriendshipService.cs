using UserService.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Services.Interfaces;

public interface IFriendshipService
{
    Task<IEnumerable<FriendDto>> GetUserFriendsAsync(string userId);
    Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(string userId);
    Task<IEnumerable<AvailableUserDto>> GetAvailableUsersAsync(string userId);
    Task<FriendshipDto> SendFriendRequestAsync(string senderId, string receiverId);
    Task<FriendshipDto> AcceptFriendRequestAsync(Guid friendshipId, string userId);
    Task<FriendshipDto> RejectFriendRequestAsync(Guid friendshipId, string userId);
    Task RemoveFriendAsync(string userId, string friendUserId);
    Task<bool> AreFriendsAsync(string userId1, string userId2);

    // HttpContext-aware methods for controller use
    Task<IActionResult> GetFriendsAsync(HttpContext httpContext);
    Task<IActionResult> GetPendingRequestsAsync(HttpContext httpContext);
    Task<IActionResult> GetAvailableUsersAsync(HttpContext httpContext);
    Task<IActionResult> SendFriendRequestAsync(HttpContext httpContext, CreateFriendRequestDto request);
    Task<IActionResult> AcceptFriendRequestAsync(HttpContext httpContext, Guid friendshipId);
    Task<IActionResult> RejectFriendRequestAsync(HttpContext httpContext, Guid friendshipId);
    Task<IActionResult> RemoveFriendAsync(HttpContext httpContext, string friendUserId);
    Task<IActionResult> CheckFriendshipAsync(HttpContext httpContext, string userId);
} 