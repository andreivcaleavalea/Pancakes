using BlogService.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogService.Services.Interfaces;

public interface ISavedBlogService
{
    Task<IEnumerable<SavedBlogDto>> GetSavedBlogsByUserIdAsync(string userId);
    Task<SavedBlogDto> SaveBlogAsync(string userId, CreateSavedBlogDto createDto);
    Task UnsaveBlogAsync(string userId, Guid blogPostId);
    Task<bool> IsBookmarkedAsync(string userId, Guid blogPostId);

    // HttpContext-aware methods for controller use
    Task<IActionResult> GetSavedBlogsAsync(HttpContext httpContext);
    Task<IActionResult> SaveBlogAsync(HttpContext httpContext, CreateSavedBlogDto createDto, ModelStateDictionary modelState);
    Task<IActionResult> UnsaveBlogAsync(HttpContext httpContext, Guid blogPostId);
    Task<IActionResult> IsBookmarkedAsync(HttpContext httpContext, Guid blogPostId);
}

