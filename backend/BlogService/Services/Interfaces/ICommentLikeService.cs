using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface ICommentLikeService
{
    Task<CommentLikeStatsDto> GetLikeStatsAsync(Guid commentId, string? UserId = null);
    Task<CommentLikeDto> CreateOrUpdateLikeAsync(CreateCommentLikeDto createDto);
    Task DeleteLikeAsync(Guid commentId, string UserId);
} 