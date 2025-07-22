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
    private readonly IUserServiceClient _userServiceClient;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BlogPostService(
        IBlogPostRepository blogPostRepository,
        IUserServiceClient userServiceClient,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _blogPostRepository = blogPostRepository;
        _userServiceClient = userServiceClient;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BlogPostDto?> GetByIdAsync(Guid id)
    {
        var blogPost = await _blogPostRepository.GetByIdAsync(id);
        if (blogPost == null) return null;
        
        var blogPostDto = _mapper.Map<BlogPostDto>(blogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        return blogPostDto;
    }

    public async Task<PaginatedResult<BlogPostDto>> GetAllAsync(BlogPostQueryParameters parameters)
    {
        var (posts, totalCount) = await _blogPostRepository.GetAllAsync(parameters);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // Populate author information for all posts
        foreach (var dto in blogPostDtos)
        {
            await PopulateAuthorInfoAsync(dto);
        }
        
        return PaginationHelper.CreatePaginatedResult(blogPostDtos, parameters.Page, parameters.PageSize, totalCount);
    }

    public async Task<IEnumerable<BlogPostDto>> GetFeaturedAsync(int count = 5)
    {
        var posts = await _blogPostRepository.GetFeaturedAsync(count);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // Populate author information for all posts
        foreach (var dto in blogPostDtos)
        {
            await PopulateAuthorInfoAsync(dto);
        }
        
        return blogPostDtos;
    }

    public async Task<IEnumerable<BlogPostDto>> GetPopularAsync(int count = 5)
    {
        var posts = await _blogPostRepository.GetPopularAsync(count);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // Populate author information for all posts
        foreach (var dto in blogPostDtos)
        {
            await PopulateAuthorInfoAsync(dto);
        }
        
        return blogPostDtos;
    }

    public async Task<IEnumerable<BlogPostDto>> GetByAuthorAsync(string authorId, int page = 1, int pageSize = 10)
    {
        // Convert string authorId to Guid for repository call
        if (!Guid.TryParse(authorId, out var authorGuid))
        {
            throw new ArgumentException($"Invalid author ID format: {authorId}");
        }
        
        var posts = await _blogPostRepository.GetByAuthorAsync(authorGuid, page, pageSize);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // Populate author information for all posts
        foreach (var dto in blogPostDtos)
        {
            await PopulateAuthorInfoAsync(dto);
        }
        
        return blogPostDtos;
    }

    public async Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto)
    {
        var blogPost = _mapper.Map<BlogPost>(createDto);
        var createdBlogPost = await _blogPostRepository.CreateAsync(blogPost);
        var blogPostDto = _mapper.Map<BlogPostDto>(createdBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        return blogPostDto;
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

    private async Task PopulateAuthorInfoAsync(BlogPostDto blogPostDto)
    {
        try
        {
            // Try to get the current user's auth token from the HTTP context
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
            string? token = null;
            
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }

            UserInfoDto? user = null;
            
            if (!string.IsNullOrEmpty(token))
            {
                // Use authenticated endpoint with token
                user = await _userServiceClient.GetUserByIdAsync(blogPostDto.AuthorId.ToString(), token);
            }
            else
            {
                // Use public endpoint without token
                user = await _userServiceClient.GetUserByIdAsync(blogPostDto.AuthorId.ToString());
            }
            
            if (user != null)
            {
                blogPostDto.AuthorName = user.Name;
                blogPostDto.AuthorImage = user.Image; // UserInfoDto.Image is already a string
            }
            else
            {
                // Handle the case where user doesn't exist (e.g., demo data)
                if (blogPostDto.AuthorId == "DEMO-AUTHOR-0000-0000-000000000000")
                {
                    blogPostDto.AuthorName = "Demo Author";
                    blogPostDto.AuthorImage = string.Empty;
                }
                else
                {
                    blogPostDto.AuthorName = "Unknown Author";
                    blogPostDto.AuthorImage = string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            // Log the actual error for debugging
            // If we can't get user info, use defaults
            if (blogPostDto.AuthorId == "DEMO-AUTHOR-0000-0000-000000000000")
            {
                blogPostDto.AuthorName = "Demo Author";
                blogPostDto.AuthorImage = string.Empty;
            }
            else
            {
                blogPostDto.AuthorName = "Unknown Author";
                blogPostDto.AuthorImage = string.Empty;
            }
        }
    }
}
