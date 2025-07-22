using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Helpers;

namespace BlogService.Services.Interfaces;

public interface IBlogPostService
{
    Task<BlogPostDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<BlogPostDto>> GetAllAsync(BlogPostQueryParameters parameters);
    Task<IEnumerable<BlogPostDto>> GetFeaturedAsync(int count = 1);
    Task<IEnumerable<BlogPostDto>> GetPopularAsync(int count = 3);
    Task<IEnumerable<BlogPostDto>> GetByAuthorAsync(string authorId, int page = 1, int pageSize = 10);
    Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto);
    Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto);
    Task DeleteAsync(Guid id);
    Task<BlogPostDto> UpdateStatusAsync(Guid id, PostStatus status);
}
