namespace BlogService.Repositories.Interfaces;

public interface IFriendshipRepository
{
    /// <summary>
    /// Get a list of user IDs that are friends with the specified user
    /// </summary>
    /// <param name="userId">The user ID to get friends for</param>
    /// <returns>List of friend user IDs</returns>
    Task<IEnumerable<string>> GetUserFriendsAsync(string userId);
}
