using BlogService.Models.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogService.Services.Interfaces;

public interface ICommentService
{
    // Original methods (still needed for backward compatibility)
    Task<CommentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CommentDto>> GetByBlogPostIdAsync(Guid blogPostId);
    Task<CommentDto> CreateAsync(CreateCommentDto createDto);
    Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto);
    Task DeleteAsync(Guid id);
    
    // New methods that handle business logic internally
    Task<CommentDto> CreateAsync(CreateCommentDto createDto, HttpContext httpContext, ModelStateDictionary modelState);
    Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto, HttpContext httpContext, ModelStateDictionary modelState);
    Task DeleteAsync(Guid id, HttpContext httpContext);
} 