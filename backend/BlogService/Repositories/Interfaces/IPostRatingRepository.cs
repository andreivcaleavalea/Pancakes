using BlogService.Models.Entities;

namespace BlogService.Repositories.Interfaces;

public interface IPostRatingRepository
{
    Task<PostRating?> GetByIdAsync(Guid id);
    Task<PostRating?> GetByBlogPostAndUserAsync(Guid blogPostId, string userId);
    Task<IEnumerable<PostRating>> GetByBlogPostIdAsync(Guid blogPostId);
    Task<PostRating> CreateAsync(PostRating rating);
    Task<PostRating> UpdateAsync(PostRating rating);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<decimal> GetAverageRatingAsync(Guid blogPostId);
    Task<int> GetTotalRatingsAsync(Guid blogPostId);
    Task<Dictionary<decimal, int>> GetRatingDistributionAsync(Guid blogPostId);
    Task<IEnumerable<PostRating>> GetUserRatingsAsync(string userId);
    Task<IEnumerable<PostRating>> GetPostRatingsByUsersAsync(Guid postId, IEnumerable<string> userIds);
    
    // Batch methods for performance optimization
    Task<Dictionary<Guid, decimal>> GetAverageRatingsBatchAsync(IEnumerable<Guid> blogPostIds);
    Task<Dictionary<Guid, int>> GetTotalRatingsBatchAsync(IEnumerable<Guid> blogPostIds);
    Task<Dictionary<Guid, (decimal Average, int Total)>> GetRatingStatsBatchAsync(IEnumerable<Guid> blogPostIds);
} 