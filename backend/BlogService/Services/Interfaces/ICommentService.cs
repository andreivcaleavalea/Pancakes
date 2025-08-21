using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface ICommentService
{
    // Core methods for comment operations
    Task<CommentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CommentDto>> GetByBlogPostIdAsync(Guid blogPostId);
    Task<CommentDto> CreateAsync(CreateCommentDto createDto, HttpContext httpContext);
    Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto, HttpContext httpContext);
    Task DeleteAsync(Guid id, HttpContext httpContext);
} 