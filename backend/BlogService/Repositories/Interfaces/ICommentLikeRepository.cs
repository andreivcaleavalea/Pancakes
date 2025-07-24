using BlogService.Models.Entities;

namespace BlogService.Repositories.Interfaces;

public interface ICommentLikeRepository
{
    Task<CommentLike?> GetByIdAsync(Guid id);
    Task<CommentLike?> GetByCommentAndUserAsync(Guid commentId, string UserId);
    Task<IEnumerable<CommentLike>> GetByCommentIdAsync(Guid commentId);
    Task<CommentLike> CreateAsync(CommentLike like);
    Task<CommentLike> UpdateAsync(CommentLike like);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetLikeCountAsync(Guid commentId);
    Task<int> GetDislikeCountAsync(Guid commentId);
} 