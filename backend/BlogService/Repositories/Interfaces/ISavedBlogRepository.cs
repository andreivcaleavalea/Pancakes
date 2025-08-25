using BlogService.Models.Entities;

namespace BlogService.Repositories.Interfaces;

public interface ISavedBlogRepository
{
    Task<IEnumerable<SavedBlog>> GetSavedBlogsByUserIdAsync(string userId);
    Task<SavedBlog?> GetSavedBlogAsync(string userId, Guid blogPostId);
    Task<SavedBlog> SaveBlogAsync(SavedBlog savedBlog);
    Task DeleteSavedBlogAsync(string userId, Guid blogPostId);
    Task<bool> IsBookmarkedAsync(string userId, Guid blogPostId);
    Task<IEnumerable<SavedBlog>> GetUserSavedPostsAsync(string userId);
    Task<IEnumerable<SavedBlog>> GetPostSavesByUsersAsync(Guid postId, IEnumerable<string> userIds);
}