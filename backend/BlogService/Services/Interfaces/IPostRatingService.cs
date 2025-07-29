using BlogService.Models.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogService.Services.Interfaces;

public interface IPostRatingService
{
    // Original methods (still needed for backward compatibility)
    Task<PostRatingStatsDto> GetRatingStatsAsync(Guid blogPostId, string? UserId = null);
    Task<PostRatingDto> CreateOrUpdateRatingAsync(CreatePostRatingDto createDto);
    Task DeleteRatingAsync(Guid blogPostId, string UserId);
    
    // New methods that handle business logic internally
    Task<PostRatingStatsDto> GetRatingStatsAsync(Guid blogPostId, HttpContext httpContext);
    Task<PostRatingDto> CreateOrUpdateRatingAsync(CreatePostRatingDto createDto, HttpContext httpContext, ModelStateDictionary modelState);
    Task DeleteRatingAsync(Guid blogPostId, HttpContext httpContext);
} 