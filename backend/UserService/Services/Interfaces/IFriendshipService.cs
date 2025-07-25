using UserService.Models.DTOs;

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
} 