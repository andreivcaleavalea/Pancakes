using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface IFriendshipService
{
    Task<IEnumerable<FriendDto>> GetUserFriendsAsync(string userId);
    Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(string userId);
    Task<IEnumerable<UserInfoDto>> GetAvailableUsersAsync(string userId);
    Task<FriendshipDto> SendFriendRequestAsync(string senderId, string receiverId);
    Task<FriendshipDto> AcceptFriendRequestAsync(Guid friendshipId, string userId);
    Task<FriendshipDto> RejectFriendRequestAsync(Guid friendshipId, string userId);
    Task RemoveFriendAsync(string userId1, string userId2);
    Task<bool> AreFriendsAsync(string userId1, string userId2);
} 