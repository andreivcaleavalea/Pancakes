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
    private readonly IAuthorizationService _authorizationService;
    private readonly IModelValidationService _modelValidationService;
    private readonly IJwtUserService _jwtUserService;
    private readonly ILogger<BlogPostService> _logger;

    public BlogPostService(
        IBlogPostRepository blogPostRepository,
        IUserServiceClient userServiceClient,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IModelValidationService modelValidationService,
        IJwtUserService jwtUserService,
        ILogger<BlogPostService> logger)
    {
        _blogPostRepository = blogPostRepository;
        _userServiceClient = userServiceClient;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _modelValidationService = modelValidationService;
        _jwtUserService = jwtUserService;
        _logger = logger;
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

    public async Task<PaginatedResult<BlogPostDto>> GetFriendsPostsAsync(IEnumerable<string> friendUserIds, int page = 1, int pageSize = 10)
    {
        var (posts, totalCount) = await _blogPostRepository.GetFriendsPostsAsync(friendUserIds, page, pageSize);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // Populate author information for all posts
        foreach (var dto in blogPostDtos)
        {
            await PopulateAuthorInfoAsync(dto);
        }
        
        return PaginationHelper.CreatePaginatedResult(blogPostDtos, page, pageSize, totalCount);
    }

    // Original methods for internal use and backwards compatibility
    public async Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto)
    {
        var blogPost = _mapper.Map<BlogPost>(createDto);
        var createdBlogPost = await _blogPostRepository.CreateAsync(blogPost);
        var blogPostDto = _mapper.Map<BlogPostDto>(createdBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        return blogPostDto;
    }

    public async Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto, string authorId)
    {
        createDto.AuthorId = authorId;
        var blogPost = _mapper.Map<BlogPost>(createDto);
        var createdBlogPost = await _blogPostRepository.CreateAsync(blogPost);
        var blogPostDto = _mapper.Map<BlogPostDto>(createdBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        return blogPostDto;
    }

    // New methods with full business logic
    public async Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto, HttpContext httpContext)
    {
        _logger.LogInformation("Creating blog post: Title={Title}", createDto?.Title ?? "null");

        // Get current user using authorization service
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        _logger.LogInformation("Current user retrieved: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        // Override AuthorId with current user's ID to ensure security
        createDto.AuthorId = currentUser.Id;
        _logger.LogInformation("Updated createDto.AuthorId to: {AuthorId}", createDto.AuthorId);

        var blogPost = _mapper.Map<BlogPost>(createDto);
        var createdBlogPost = await _blogPostRepository.CreateAsync(blogPost);
        var blogPostDto = _mapper.Map<BlogPostDto>(createdBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        
        _logger.LogInformation("Blog post created successfully with ID: {BlogPostId}", blogPostDto.Id);
        return blogPostDto;
    }

    public async Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto, string currentUserId)
    {
        var existingBlogPost = await GetBlogPostByIdOrThrowAsync(id);
        
        // Check if the current user is the author of the blog post
        if (existingBlogPost.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only edit your own blog posts.");
        }
        
        _mapper.Map(updateDto, existingBlogPost);
        existingBlogPost.UpdatedAt = DateTime.UtcNow;
        var updatedBlogPost = await _blogPostRepository.UpdateAsync(existingBlogPost);
        var blogPostDto = _mapper.Map<BlogPostDto>(updatedBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        return blogPostDto;
    }

    public async Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto, HttpContext httpContext)
    {
        _logger.LogInformation("Updating blog post with ID: {Id}", id);

        // Get current user using authorization service
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        _logger.LogInformation("Current user retrieved for update: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        var existingBlogPost = await GetBlogPostByIdOrThrowAsync(id);
        
        // Check if the current user is the author of the blog post
        if (existingBlogPost.AuthorId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You can only edit your own blog posts.");
        }
        
        _mapper.Map(updateDto, existingBlogPost);
        existingBlogPost.UpdatedAt = DateTime.UtcNow;
        var updatedBlogPost = await _blogPostRepository.UpdateAsync(existingBlogPost);
        var blogPostDto = _mapper.Map<BlogPostDto>(updatedBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        return blogPostDto;
    }

    public async Task DeleteAsync(Guid id, string currentUserId)
    {
        var existingBlogPost = await GetBlogPostByIdOrThrowAsync(id);
        
        // Check if the current user is the author of the blog post
        if (existingBlogPost.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own blog posts.");
        }
        
        await _blogPostRepository.DeleteAsync(id);
    }

    public async Task DeleteAsync(Guid id, HttpContext httpContext)
    {
        _logger.LogInformation("Deleting blog post with ID: {Id}", id);

        // Get current user using authorization service
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        _logger.LogInformation("Current user retrieved for delete: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        var existingBlogPost = await GetBlogPostByIdOrThrowAsync(id);
        
        // Check if the current user is the author of the blog post
        if (existingBlogPost.AuthorId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You can only delete your own blog posts.");
        }
        
        await _blogPostRepository.DeleteAsync(id);
    }

    // Internal methods for backwards compatibility (used by UpdateStatusAsync)
    private async Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto)
    {
        var existingBlogPost = await GetBlogPostByIdOrThrowAsync(id);
        _mapper.Map(updateDto, existingBlogPost);
        existingBlogPost.UpdatedAt = DateTime.UtcNow;
        var updatedBlogPost = await _blogPostRepository.UpdateAsync(existingBlogPost);
        var blogPostDto = _mapper.Map<BlogPostDto>(updatedBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        return blogPostDto;
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
                
                // Convert relative image path to full URL pointing to UserService
                if (!string.IsNullOrEmpty(user.Image))
                {
                    var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:5141";
                    if (user.Image.StartsWith("assets/profile-pictures/"))
                    {
                        // Convert relative path to full URL
                        var filename = Path.GetFileName(user.Image);
                        blogPostDto.AuthorImage = $"{userServiceUrl}/assets/profile-pictures/{filename}";
                    }
                    else
                    {
                        // If it's already a full URL or different format, use as is
                        blogPostDto.AuthorImage = user.Image;
                    }
                }
                else
                {
                    blogPostDto.AuthorImage = string.Empty;
                }
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
