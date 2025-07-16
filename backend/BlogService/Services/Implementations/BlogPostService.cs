using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using BlogService.Helpers;

namespace BlogService.Services.Implementations;

public class BlogPostService : IBlogPostService
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;

    public BlogPostService(
        IBlogPostRepository blogPostRepository,
        IMapper mapper)
    {
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
    }

    public async Task<BlogPostDto?> GetByIdAsync(Guid id)
    {
        var blogPost = await _blogPostRepository.GetByIdAsync(id);
        return blogPost != null ? _mapper.Map<BlogPostDto>(blogPost) : null;
    }

    public async Task<PaginatedResult<BlogPostDto>> GetAllAsync(BlogPostQueryParameters parameters)
    {
        var (posts, totalCount) = await _blogPostRepository.GetAllAsync(parameters);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        return PaginationHelper.CreatePaginatedResult(blogPostDtos, parameters.Page, parameters.PageSize, totalCount);
    }

    public async Task<IEnumerable<BlogPostDto>> GetFeaturedAsync(int count = 5)
    {
        var posts = await _blogPostRepository.GetFeaturedAsync(count);
        return _mapper.Map<IEnumerable<BlogPostDto>>(posts);
    }

    public async Task<IEnumerable<BlogPostDto>> GetPopularAsync(int count = 5)
    {
        var posts = await _blogPostRepository.GetPopularAsync(count);
        return _mapper.Map<IEnumerable<BlogPostDto>>(posts);
    }

    public async Task<IEnumerable<BlogPostDto>> GetByAuthorAsync(Guid authorId, int page = 1, int pageSize = 10)
    {
        var posts = await _blogPostRepository.GetByAuthorAsync(authorId, page, pageSize);
        return _mapper.Map<IEnumerable<BlogPostDto>>(posts);
    }

    public async Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto)
    {
        var blogPost = _mapper.Map<BlogPost>(createDto);
        var createdBlogPost = await _blogPostRepository.CreateAsync(blogPost);
        return _mapper.Map<BlogPostDto>(createdBlogPost);
    }

    public async Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto)
    {
        var existingBlogPost = await GetBlogPostByIdOrThrowAsync(id);
        _mapper.Map(updateDto, existingBlogPost);
        var updatedBlogPost = await _blogPostRepository.UpdateAsync(existingBlogPost);
        return _mapper.Map<BlogPostDto>(updatedBlogPost);
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetBlogPostByIdOrThrowAsync(id);
        await _blogPostRepository.DeleteAsync(id);
    }

    public async Task<BlogPostDto> UpdateStatusAsync(Guid id, PostStatus status)
    {
        var blogPost = await GetBlogPostByIdOrThrowAsync(id);
        blogPost.Status = status;
        if (status == PostStatus.Published && !blogPost.PublishedAt.HasValue)
        {
            blogPost.PublishedAt = DateTime.UtcNow;
        }
        var updatedBlogPost = await _blogPostRepository.UpdateAsync(blogPost);
        return _mapper.Map<BlogPostDto>(updatedBlogPost);
    }

    private async Task<BlogPost> GetBlogPostByIdOrThrowAsync(Guid id)
    {
        var blogPost = await _blogPostRepository.GetByIdAsync(id);
        if (blogPost == null)
        {
            throw new ArgumentException($"Blog post with ID {id} not found.");
        }
        return blogPost;
    }
}
