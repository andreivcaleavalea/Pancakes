using BlogService.Models.Entities;

namespace BlogService.Repositories.Interfaces;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Comment>> GetByBlogPostIdAsync(Guid blogPostId);
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment> UpdateAsync(Comment comment);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> HasRepliesAsync(Guid commentId);
} 