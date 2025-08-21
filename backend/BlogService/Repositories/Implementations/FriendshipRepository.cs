using BlogService.Repositories.Interfaces;

namespace BlogService.Repositories.Implementations;

public class FriendshipRepository : IFriendshipRepository
{
    // TODO: This is a placeholder implementation
    // In a real scenario, this should either:
    // 1. Call the UserService API to get friendship data
    // 2. Have direct access to the UserService database
    // 3. Use a shared friendship data store

    public async Task<IEnumerable<string>> GetUserFriendsAsync(string userId)
    {
        // Placeholder - return empty for now
        // In production, this would call UserService or access shared data
        await Task.CompletedTask;
        return Enumerable.Empty<string>();
    }
}
