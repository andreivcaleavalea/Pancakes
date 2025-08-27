using BlogService.Models.Entities;

namespace BlogService.Services.Interfaces;

public interface IRecommendationService
{
    /// <summary>
    /// Get personalized blog post recommendations for a user
    /// </summary>
    /// <param name="userId">User ID to get recommendations for</param>
    /// <param name="count">Number of recommendations to return</param>
    /// <param name="excludeAuthorId">Author ID to exclude from recommendations (usually the requesting user)</param>
    /// <returns>List of recommended blog posts</returns>
    Task<IEnumerable<BlogPost>> GetPersonalizedRecommendationsAsync(string userId, int count = 5, string? excludeAuthorId = null);
    
    /// <summary>
    /// Get fallback recommendations when there's insufficient data for personalization
    /// Uses view count, ratings, and recency as primary factors
    /// </summary>
    /// <param name="count">Number of recommendations to return</param>
    /// <param name="excludeAuthorId">Author ID to exclude from recommendations</param>
    /// <param name="excludePostIds">Post IDs to exclude from recommendations</param>
    /// <returns>List of trending blog posts</returns>
    Task<IEnumerable<BlogPost>> GetTrendingRecommendationsAsync(int count = 5, string? excludeAuthorId = null, IEnumerable<Guid>? excludePostIds = null);
    
    /// <summary>
    /// Get popular recommendations based on overall engagement metrics
    /// Used when there are not enough blog posts for the algorithm (< 20 posts)
    /// </summary>
    /// <param name="count">Number of recommendations to return</param>
    /// <param name="excludeAuthorId">Author ID to exclude from recommendations</param>
    /// <returns>List of popular blog posts</returns>
    Task<IEnumerable<BlogPost>> GetSimplePopularRecommendationsAsync(int count = 5, string? excludeAuthorId = null);
}
