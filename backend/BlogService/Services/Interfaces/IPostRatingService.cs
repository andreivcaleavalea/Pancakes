using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface IPostRatingService
{
    Task<PostRatingStatsDto> GetRatingStatsAsync(Guid blogPostId, string? userIdentifier = null);
    Task<PostRatingDto> CreateOrUpdateRatingAsync(CreatePostRatingDto createDto);
    Task DeleteRatingAsync(Guid blogPostId, string userIdentifier);
} 