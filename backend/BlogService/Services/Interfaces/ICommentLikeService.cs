using BlogService.Models.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogService.Services.Interfaces;

public interface ICommentLikeService
{
    // Original methods (still needed for backward compatibility)
    Task<CommentLikeStatsDto> GetLikeStatsAsync(Guid commentId, string? UserId = null);
    Task<CommentLikeDto> CreateOrUpdateLikeAsync(CreateCommentLikeDto createDto);
    Task DeleteLikeAsync(Guid commentId, string UserId);
    
    // New methods that handle business logic internally
    Task<CommentLikeStatsDto> GetLikeStatsAsync(Guid commentId, HttpContext httpContext);
    Task<CommentLikeDto> CreateOrUpdateLikeAsync(CreateCommentLikeDto createDto, HttpContext httpContext, ModelStateDictionary modelState);
    Task DeleteLikeAsync(Guid commentId, HttpContext httpContext);
} 