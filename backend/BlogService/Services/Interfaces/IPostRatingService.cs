using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface IPostRatingService
{
    Task<PostRatingStatsDto> GetRatingStatsAsync(Guid blogPostId, string? UserId = null);
    Task<PostRatingDto> CreateOrUpdateRatingAsync(CreatePostRatingDto createDto);
    Task DeleteRatingAsync(Guid blogPostId, string UserId);
} 