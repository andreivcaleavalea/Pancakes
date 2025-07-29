using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace BlogService.Services.Implementations;

public class SavedBlogService : ISavedBlogService
{
    private readonly ISavedBlogRepository _savedBlogRepository;
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;
    private readonly IBlogPostService _blogPostService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<SavedBlogService> _logger;

    public SavedBlogService(
        ISavedBlogRepository savedBlogRepository,
        IBlogPostRepository blogPostRepository,
        IMapper mapper,
        IBlogPostService blogPostService,
        IAuthorizationService authorizationService,
        ILogger<SavedBlogService> logger)
    {
        _savedBlogRepository = savedBlogRepository;
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
        _blogPostService = blogPostService;
        _authorizationService = authorizationService;
        _logger = logger;
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

    // HttpContext-aware methods for controller use
    public async Task<IActionResult> GetSavedBlogsAsync(HttpContext httpContext)
    {
        try
        {
            // Get current user using authorization service
            var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
            if (currentUser == null)
            {
                return new UnauthorizedObjectResult("Authorization token is required or invalid");
            }

            var savedBlogs = await GetSavedBlogsByUserIdAsync(currentUser.Id);
            return new OkObjectResult(savedBlogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving saved blogs");
            return new ObjectResult("An error occurred while retrieving saved blogs") { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> SaveBlogAsync(HttpContext httpContext, CreateSavedBlogDto createDto, ModelStateDictionary modelState)
    {
        try
        {
            if (!modelState.IsValid)
            {
                return new BadRequestObjectResult(modelState);
            }

            // Get current user using authorization service
            var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
            if (currentUser == null)
            {
                return new UnauthorizedObjectResult("Authorization token is required or invalid");
            }

            var savedBlog = await SaveBlogAsync(currentUser.Id, createDto);
            return new CreatedAtActionResult("GetSavedBlogs", null, null, savedBlog);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving blog");
            return new ObjectResult("An error occurred while saving the blog") { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> UnsaveBlogAsync(HttpContext httpContext, Guid blogPostId)
    {
        try
        {
            // Get current user using authorization service
            var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
            if (currentUser == null)
            {
                return new UnauthorizedObjectResult("Authorization token is required or invalid");
            }

            await UnsaveBlogAsync(currentUser.Id, blogPostId);
            return new NoContentResult();
        }
        catch (ArgumentException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsaving blog");
            return new ObjectResult("An error occurred while unsaving the blog") { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> IsBookmarkedAsync(HttpContext httpContext, Guid blogPostId)
    {
        try
        {
            // Get current user using authorization service
            var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
            if (currentUser == null)
            {
                return new UnauthorizedObjectResult("Authorization token is required or invalid");
            }

            var isBookmarked = await IsBookmarkedAsync(currentUser.Id, blogPostId);
            return new OkObjectResult(new { isBookmarked });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking bookmark status");
            return new ObjectResult("An error occurred while checking bookmark status") { StatusCode = 500 };
        }
    }
}