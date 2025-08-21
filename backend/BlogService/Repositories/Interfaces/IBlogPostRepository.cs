using BlogService.Models.Entities;
using BlogService.Models.Requests;

namespace BlogService.Repositories.Interfaces;

public interface IBlogPostRepository
{
    Task<BlogPost?> GetByIdAsync(Guid id);
    Task<(IEnumerable<BlogPost> posts, int totalCount)> GetAllAsync(BlogPostQueryParameters parameters);
    Task<IEnumerable<BlogPost>> GetFeaturedAsync(int count = 1);
    Task<IEnumerable<BlogPost>> GetPopularAsync(int count = 3);
    Task<(IEnumerable<BlogPost> posts, int totalCount)> GetFriendsPostsAsync(IEnumerable<string> friendUserIds, int page = 1, int pageSize = 10);
    Task<BlogPost> CreateAsync(BlogPost blogPost);
    Task<BlogPost> UpdateAsync(BlogPost blogPost);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task IncrementViewCountAsync(Guid id);
    Task<int> GetTotalPublishedCountAsync();
}
