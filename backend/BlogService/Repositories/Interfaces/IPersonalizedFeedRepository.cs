using BlogService.Models.Entities;

namespace BlogService.Repositories.Interfaces;

public interface IPersonalizedFeedRepository
{
    /// <summary>
    /// Get personalized feed for a user
    /// </summary>
    Task<PersonalizedFeed?> GetUserFeedAsync(string userId);
    
    /// <summary>
    /// Create or update a user's personalized feed
    /// </summary>
    Task<PersonalizedFeed> UpsertUserFeedAsync(string userId, List<Guid> blogPostIds, List<double> scores, string algorithmVersion = "1.0");
    
    /// <summary>
    /// Get all expired feeds that need recomputation
    /// </summary>
    Task<IEnumerable<PersonalizedFeed>> GetExpiredFeedsAsync();
    
    /// <summary>
    /// Get feeds that are close to expiring (for proactive refresh)
    /// </summary>
    Task<IEnumerable<PersonalizedFeed>> GetExpiringFeedsAsync(int minutesUntilExpiry = 5);
    
    /// <summary>
    /// Get all user IDs that don't have feeds yet
    /// </summary>
    Task<IEnumerable<string>> GetUsersWithoutFeedsAsync();
    
    /// <summary>
    /// Delete old feeds (cleanup)
    /// </summary>
    Task DeleteExpiredFeedsAsync(int daysOld = 7);
    
    /// <summary>
    /// Get feed statistics for monitoring
    /// </summary>
    Task<(int totalFeeds, int validFeeds, int expiredFeeds)> GetFeedStatisticsAsync();
}
