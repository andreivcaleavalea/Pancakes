using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface ISavedBlogService
{
    Task<IEnumerable<SavedBlogDto>> GetSavedBlogsByUserIdAsync(string userId);
    Task<SavedBlogDto> SaveBlogAsync(string userId, CreateSavedBlogDto createDto);
    Task UnsaveBlogAsync(string userId, Guid blogPostId);
    Task<bool> IsBookmarkedAsync(string userId, Guid blogPostId);
}

