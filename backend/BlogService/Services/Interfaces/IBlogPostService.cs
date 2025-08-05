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
    Task<IEnumerable<BlogPostDto>> GetByAuthorAsync(string authorId, int page = 1, int pageSize = 10);
    Task<PaginatedResult<BlogPostDto>> GetFriendsPostsAsync(IEnumerable<string> friendUserIds, int page = 1, int pageSize = 10);
    
    // Original methods (still needed by some controllers)
    Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto, string authorId);
    Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto, string currentUserId);
    Task DeleteAsync(Guid id, string currentUserId);
    
    // Updated methods to handle authorization and validation internally
    Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto, HttpContext httpContext);
    Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto, HttpContext httpContext);
    Task DeleteAsync(Guid id, HttpContext httpContext);
    
    Task<BlogPostDto> UpdateStatusAsync(Guid id, PostStatus status);
}
