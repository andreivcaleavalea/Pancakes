using BlogService.Models.Entities;

namespace BlogService.Repositories.Interfaces;

public interface IFriendshipRepository
{
    Task<Friendship?> GetByIdAsync(Guid id);
    Task<Friendship?> GetFriendshipAsync(string senderId, string receiverId);
    Task<IEnumerable<Friendship>> GetUserFriendsAsync(string userId);
    Task<IEnumerable<Friendship>> GetPendingRequestsReceivedAsync(string userId);
    Task<IEnumerable<Friendship>> GetPendingRequestsSentAsync(string userId);
    Task<IEnumerable<string>> GetAllUsersExceptFriendsAsync(string userId);
    Task<Friendship> CreateAsync(Friendship friendship);
    Task<Friendship> UpdateAsync(Friendship friendship);
    Task DeleteAsync(Guid id);
    Task<bool> AreFriendsAsync(string userId1, string userId2);
} 