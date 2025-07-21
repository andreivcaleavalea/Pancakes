using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface ICommentService
{
    Task<CommentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CommentDto>> GetByBlogPostIdAsync(Guid blogPostId);
    Task<CommentDto> CreateAsync(CreateCommentDto createDto);
    Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto);
    Task DeleteAsync(Guid id);
} 