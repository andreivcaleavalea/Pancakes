using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Helpers;

namespace BlogService.Services.Interfaces;

public interface IBlogPostService
{
    Task<BlogPostDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<BlogPostDto>> GetAllAsync(BlogPostQueryParameters parameters);
    Task<PaginatedResult<BlogPostDto>> GetAllAsync(BlogPostQueryParameters parameters, HttpContext httpContext);
    Task<IEnumerable<BlogPostDto>> GetFeaturedAsync(int count = 1);
    Task<IEnumerable<BlogPostDto>> GetPopularAsync(int count = 3);
    Task<IEnumerable<BlogPostDto>> GetPersonalizedPopularAsync(int count = 3, HttpContext? httpContext = null);
    Task<PaginatedResult<BlogPostDto>> GetFriendsPostsAsync(IEnumerable<string> friendUserIds, int page = 1, int pageSize = 10);
    
    // Updated methods to handle authorization and validation internally
    Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto, HttpContext httpContext);
    Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto, HttpContext httpContext);
    Task DeleteAsync(Guid id, HttpContext httpContext);
    
    // Draft-specific methods
    Task<PaginatedResult<BlogPostDto>> GetUserDraftsAsync(HttpContext httpContext, int page = 1, int pageSize = 10);
    Task<BlogPostDto> ConvertToDraftAsync(Guid id, HttpContext httpContext);
    Task<BlogPostDto> PublishDraftAsync(Guid id, HttpContext httpContext);
    
    // View tracking
    Task IncrementViewCountAsync(Guid id);
}
