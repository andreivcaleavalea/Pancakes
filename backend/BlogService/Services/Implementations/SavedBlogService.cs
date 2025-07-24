using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class SavedBlogService : ISavedBlogService
{
    private readonly ISavedBlogRepository _savedBlogRepository;
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;
    private readonly IBlogPostService _blogPostService;

    public SavedBlogService(
        ISavedBlogRepository savedBlogRepository,
        IBlogPostRepository blogPostRepository,
        IMapper mapper,
        IBlogPostService blogPostService)
    {
        _savedBlogRepository = savedBlogRepository;
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
        _blogPostService = blogPostService;
    }

    public async Task<IEnumerable<SavedBlogDto>> GetSavedBlogsByUserIdAsync(string userId)
    {
        var savedBlogs = await _savedBlogRepository.GetSavedBlogsByUserIdAsync(userId);
        var savedBlogDtos = new List<SavedBlogDto>();

        foreach (var savedBlog in savedBlogs)
        {
            var savedBlogDto = _mapper.Map<SavedBlogDto>(savedBlog);
            
            // Get the full blog post details using the blog post service
            // This ensures author info and other details are populated
            var blogPostDto = await _blogPostService.GetByIdAsync(savedBlog.BlogPostId);
            savedBlogDto.BlogPost = blogPostDto;
            
            savedBlogDtos.Add(savedBlogDto);
        }

        return savedBlogDtos;
    }

    public async Task<SavedBlogDto> SaveBlogAsync(string userId, CreateSavedBlogDto createDto)
    {
        // Check if blog post exists
        var blogPostExists = await _blogPostRepository.ExistsAsync(createDto.BlogPostId);
        if (!blogPostExists)
        {
            throw new ArgumentException($"Blog post with ID {createDto.BlogPostId} not found.");
        }

        // Check if already saved
        var existingSave = await _savedBlogRepository.GetSavedBlogAsync(userId, createDto.BlogPostId);
        if (existingSave != null)
        {
            throw new ArgumentException("Blog post is already saved.");
        }

        var savedBlog = new SavedBlog
        {
            UserId = userId,
            BlogPostId = createDto.BlogPostId
        };

        var createdSavedBlog = await _savedBlogRepository.SaveBlogAsync(savedBlog);
        var savedBlogDto = _mapper.Map<SavedBlogDto>(createdSavedBlog);
        
        // Get the full blog post details
        var blogPostDto = await _blogPostService.GetByIdAsync(createdSavedBlog.BlogPostId);
        savedBlogDto.BlogPost = blogPostDto;
        
        return savedBlogDto;
    }

    public async Task UnsaveBlogAsync(string userId, Guid blogPostId)
    {
        var existingSave = await _savedBlogRepository.GetSavedBlogAsync(userId, blogPostId);
        if (existingSave == null)
        {
            throw new ArgumentException("Blog post is not saved.");
        }

        await _savedBlogRepository.DeleteSavedBlogAsync(userId, blogPostId);
    }

    public async Task<bool> IsBookmarkedAsync(string userId, Guid blogPostId)
    {
        return await _savedBlogRepository.IsBookmarkedAsync(userId, blogPostId);
    }
}