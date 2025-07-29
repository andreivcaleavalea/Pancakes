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
} 