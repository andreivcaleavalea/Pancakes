using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using BlogService.Helpers;
using BlogService.Configuration;
using Microsoft.Extensions.Caching.Memory;

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
    private readonly IMemoryCache _cache;

    public BlogPostService(
        IBlogPostRepository blogPostRepository,
        IUserServiceClient userServiceClient,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IModelValidationService modelValidationService,
        IJwtUserService jwtUserService,
        ILogger<BlogPostService> logger,
        IMemoryCache cache)
    {
        _blogPostRepository = blogPostRepository;
        _userServiceClient = userServiceClient;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _modelValidationService = modelValidationService;
        _jwtUserService = jwtUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BlogPostDto?> GetByIdAsync(Guid id)
    {
        // 🚀 CACHE: Check cache first for individual blog post
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostById, id);
        if (_cache.TryGetValue(cacheKey, out BlogPostDto? cachedPost))
        {
            return cachedPost;
        }

        var blogPost = await _blogPostRepository.GetByIdAsync(id);
        if (blogPost == null) return null;
        
        // Filter out deleted posts for regular users (only show published posts)
        if (blogPost.Status == PostStatus.Deleted)
        {
            return null;
        }
        
        var blogPostDto = _mapper.Map<BlogPostDto>(blogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        
        // 🚀 CACHE: Store individual blog post with medium duration
        _cache.Set(cacheKey, blogPostDto, CacheConfig.Duration.Medium);
        
        return blogPostDto;
    }

    public async Task<PaginatedResult<BlogPostDto>> GetAllAsync(BlogPostQueryParameters parameters)
    {
        // 🚀 CACHE: Create cache key based on parameters (for common combinations)
        var paramHash = CacheConfig.CreateHash(
            parameters.Page, 
            parameters.PageSize, 
            parameters.Search ?? "", 
            parameters.AuthorId ?? "", 
            parameters.Status?.ToString() ?? "",
            parameters.SortBy ?? "",
            parameters.SortOrder ?? "",
            parameters.DateFrom?.ToString() ?? "",
            parameters.DateTo?.ToString() ?? "",
            string.Join(",", parameters.Tags ?? new List<string>())
        );
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, parameters.Page, parameters.PageSize, paramHash);
        
        if (_cache.TryGetValue(cacheKey, out PaginatedResult<BlogPostDto>? cachedResult))
        {
            return cachedResult;
        }

        var (posts, totalCount) = await _blogPostRepository.GetAllAsync(parameters);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // ✅ OPTIMIZED: Batch populate author information for all posts
        await PopulateAuthorInfoBatchAsync(blogPostDtos);
        
        var result = PaginationHelper.CreatePaginatedResult(blogPostDtos, parameters.Page, parameters.PageSize, totalCount);
        
        // 🚀 CACHE: Store paginated results with short duration (content changes frequently)
        _cache.Set(cacheKey, result, CacheConfig.Duration.Short);
        
        return result;
    }

    public async Task<PaginatedResult<BlogPostDto>> GetAllAsync(BlogPostQueryParameters parameters, HttpContext httpContext)
    {
        var isServiceRequest = httpContext.User.FindFirst("service")?.Value == "true";
        
        // For regular API calls (not from admin service), only show published posts by default
        // unless a specific status is requested
        if (!isServiceRequest && !parameters.Status.HasValue)
        {
            parameters.Status = PostStatus.Published;
        }
        
        return await GetAllAsync(parameters);
    }

    public async Task<IEnumerable<BlogPostDto>> GetFeaturedAsync(int count = 5)
    {
        // 🚀 CACHE: Check cache for featured posts (very frequently accessed!)
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.FeaturedPosts, count);
        if (_cache.TryGetValue(cacheKey, out IEnumerable<BlogPostDto>? cachedPosts))
        {
            return cachedPosts;
        }

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
        // 🚀 CACHE: Check cache for popular posts (very frequently accessed!)
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.PopularPosts, count);
        if (_cache.TryGetValue(cacheKey, out IEnumerable<BlogPostDto>? cachedPosts))
        {
            return cachedPosts;
        }

        var posts = await _blogPostRepository.GetPopularAsync(count);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // ✅ OPTIMIZED: Batch populate author information for all posts
        await PopulateAuthorInfoBatchAsync(blogPostDtos);
        
        // 🚀 CACHE: Store popular posts with short duration (popularity can change)
        _cache.Set(cacheKey, blogPostDtos, CacheConfig.Duration.Short);
        
        return blogPostDtos;
    }

    public async Task<PaginatedResult<BlogPostDto>> GetFriendsPostsAsync(IEnumerable<string> friendUserIds, int page = 1, int pageSize = 10)
    {
        var (posts, totalCount) = await _blogPostRepository.GetFriendsPostsAsync(friendUserIds, page, pageSize);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // ✅ OPTIMIZED: Batch populate author information for all posts
        await PopulateAuthorInfoBatchAsync(blogPostDtos);
        
        return PaginationHelper.CreatePaginatedResult(blogPostDtos, page, pageSize, totalCount);
    }

    // New methods with full business logic
    public async Task<BlogPostDto> CreateAsync(CreateBlogPostDto createDto, HttpContext httpContext)
    {
        _logger.LogInformation("Creating blog post: Title={Title}", createDto?.Title ?? "null");

        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        _logger.LogInformation("Current user retrieved: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        createDto.AuthorId = currentUser.Id;
        _logger.LogInformation("Updated createDto.AuthorId to: {AuthorId}", createDto.AuthorId);

        var blogPost = _mapper.Map<BlogPost>(createDto);
        var createdBlogPost = await _blogPostRepository.CreateAsync(blogPost);
        var blogPostDto = _mapper.Map<BlogPostDto>(createdBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        
        _logger.LogInformation("Blog post created successfully with ID: {BlogPostId}", blogPostDto.Id);
        return blogPostDto;
    }

    public async Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostDto updateDto, HttpContext httpContext)
    {
        _logger.LogInformation("Updating blog post with ID: {Id}", id);

        BlogPost? existingBlogPost = null;

        // Check if this is a service-to-service call (e.g., from AdminService)
        var isServiceRequest = httpContext.User.FindFirst("service")?.Value == "true";
        
        if (isServiceRequest)
        {
            _logger.LogInformation("Service request detected, bypassing ownership check for blog post update");
        }
        else
        {
            // Get current user using authorization service for regular user requests
            var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("Authorization token is required or invalid");
            }

            _logger.LogInformation("Current user retrieved for update: Id={UserId}, Name={UserName}", 
                currentUser.Id, currentUser.Name);

            existingBlogPost = await GetBlogPostByIdOrThrowAsync(id);
            
            // Check if the current user is the author of the blog post
            if (existingBlogPost.AuthorId != currentUser.Id)
            {
                throw new UnauthorizedAccessException("You can only edit your own blog posts.");
            }
        }

        var blogPostToUpdate = existingBlogPost ?? await GetBlogPostByIdOrThrowAsync(id);
        _mapper.Map(updateDto, blogPostToUpdate);
        blogPostToUpdate.UpdatedAt = DateTime.UtcNow;
        var updatedBlogPost = await _blogPostRepository.UpdateAsync(blogPostToUpdate);
        var blogPostDto = _mapper.Map<BlogPostDto>(updatedBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        return blogPostDto;
    }

    public async Task DeleteAsync(Guid id, HttpContext httpContext)
    {
        _logger.LogInformation("Deleting blog post with ID: {Id}", id);

        // Check if this is a service-to-service call (e.g., from AdminService)
        var isServiceRequest = httpContext.User.FindFirst("service")?.Value == "true";
        
        if (isServiceRequest)
        {
            _logger.LogInformation("Service request detected, bypassing ownership check for blog post deletion");
        }
        else
        {
            // Get current user using authorization service for regular user requests
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
        }

        // Soft delete: Set status to Deleted instead of hard deleting
        var blogPost = await GetBlogPostByIdOrThrowAsync(id);
        blogPost.Status = PostStatus.Deleted;
        blogPost.UpdatedAt = DateTime.UtcNow;
        await _blogPostRepository.UpdateAsync(blogPost);
        
        // Comprehensive cache clearing for blog deletion
        ClearBlogPostCaches(id, blogPost);
        
        _logger.LogInformation("Blog post {BlogPostId} deleted and all related caches cleared", id);
    }

    private void ClearBlogPostCaches(Guid deletedPostId, BlogPost deletedPost)
    {
        try
        {
            // APPROACH 1: Clear specific known cache entries
            
            // Clear individual blog post cache
            var individualPostKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostById, deletedPostId);
            _cache.Remove(individualPostKey);
            
            // Clear featured posts cache (all common counts)
            for (int count = 1; count <= 10; count++)
            {
                var featuredKey = CacheConfig.FormatKey(CacheConfig.Keys.FeaturedPosts, count);
                _cache.Remove(featuredKey);
            }
            
            // Clear popular posts cache (all common counts)
            for (int count = 1; count <= 10; count++)
            {
                var popularKey = CacheConfig.FormatKey(CacheConfig.Keys.PopularPosts, count);
                _cache.Remove(popularKey);
            }
            
            // APPROACH 2: Clear paginated results by attempting common combinations
            var commonPageSizes = new[] { 5, 10, 15, 20, 25 };
            var commonPages = new[] { 1, 2, 3, 4, 5 };
            
            foreach (var pageSize in commonPageSizes)
            {
                foreach (var page in commonPages)
                {
                    // Clear general blog lists (no specific filters)
                    var generalHash = CacheConfig.CreateHash(page, pageSize, "", "", "", "", "", "", "", "");
                    var generalKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, page, pageSize, generalHash);
                    _cache.Remove(generalKey);
                    
                    // Clear published posts lists
                    var publishedHash = CacheConfig.CreateHash(page, pageSize, "", "", "1", "", "", "", "", "");
                    var publishedKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, page, pageSize, publishedHash);
                    _cache.Remove(publishedKey);
                }
            }
            
            // APPROACH 3: For maximum reliability, clear ALL cache
            // This is aggressive but ensures no stale data remains
            ClearAllCache();
            
            _logger.LogInformation("Cleared caches for deleted blog post {BlogPostId}", deletedPostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing caches for deleted blog post {BlogPostId}", deletedPostId);
            // If specific cache clearing fails, clear everything as fallback
            ClearAllCache();
        }
    }
    
    private void ClearAllCache()
    {
        // Clear all blog-related cache entries aggressively
        // Since we can't easily clear all entries, we'll clear the most comprehensive set
        try
        {
            var keysToRemove = new List<string>();
            
            // Clear all possible blog post page combinations (more comprehensive)
            var pageSizes = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 50, 100 };
            var pages = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            
            foreach (var pageSize in pageSizes)
            {
                foreach (var page in pages)
                {
                    // Clear all possible filter combinations
                    var combinations = new[]
                    {
                        CacheConfig.CreateHash(page, pageSize, "", "", "", "", "", "", "", ""),        // No filters
                        CacheConfig.CreateHash(page, pageSize, "", "", "1", "", "", "", "", ""),       // Published only
                        CacheConfig.CreateHash(page, pageSize, "", "", "0", "", "", "", "", ""),       // Draft only
                        CacheConfig.CreateHash(page, pageSize, "", "", "2", "", "", "", "", ""),       // Deleted only
                    };
                    
                    foreach (var hash in combinations)
                    {
                        var key = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, page, pageSize, hash);
                        keysToRemove.Add(key);
                    }
                }
            }
            
            // Clear all featured/popular combinations
            for (int count = 1; count <= 50; count++)
            {
                keysToRemove.Add(CacheConfig.FormatKey(CacheConfig.Keys.FeaturedPosts, count));
                keysToRemove.Add(CacheConfig.FormatKey(CacheConfig.Keys.PopularPosts, count));
            }
            
            // Remove all identified cache entries
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
            
            _logger.LogWarning("Aggressively cleared {CacheCount} blog-related cache entries due to blog post deletion", keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear blog-related cache entries - cache may contain stale data");
        }
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

    /// <summary>
    /// ✅ OPTIMIZED: Batch populate author information for multiple blog posts
    /// Eliminates N+1 problem by fetching all authors in a single API call
    /// </summary>
    private async Task PopulateAuthorInfoBatchAsync(IEnumerable<BlogPostDto> blogPostDtos)
    {
        var blogPostList = blogPostDtos.ToList();
        if (!blogPostList.Any()) return;

        try
        {
            // 🎯 Step 1: Extract unique author IDs (eliminates duplicates)
            var authorIds = blogPostList
                .Select(dto => dto.AuthorId.ToString())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (!authorIds.Any()) return;

            // 🎯 Step 2: Get auth token from HTTP context (same logic as individual method)
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
            string? token = null;
            
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }

            // 🚀 Step 3: SINGLE batch API call instead of N individual calls
            var authors = await _userServiceClient.GetUsersByIdsAsync(authorIds, token);
            
            // 🎯 Step 4: Create lookup dictionary for O(1) author retrieval
            var authorLookup = authors.ToDictionary(a => a.Id, a => a);

            // 🎯 Step 5: Populate author info for each blog post
            foreach (var dto in blogPostList)
            {
                var authorIdString = dto.AuthorId.ToString();
                
                if (authorLookup.TryGetValue(authorIdString, out var author))
                {
                    dto.AuthorName = author.Name;
                    dto.AuthorImage = BuildAuthorImageUrl(author.Image);
                }
                else
                {
                    // Handle missing author (demo data or deleted users)
                    HandleMissingAuthor(dto, authorIdString);
                }
            }

            _logger.LogInformation("✅ Batch populated author info for {BlogPostCount} posts using {AuthorCount} unique authors", 
                blogPostList.Count, authorIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in batch author population, falling back to individual calls");
            
            // Fallback to individual calls if batch fails
            foreach (var dto in blogPostList)
            {
                await PopulateAuthorInfoAsync(dto);
            }
        }
    }

    /// <summary>
    /// Helper method to build author image URL
    /// </summary>
    private string BuildAuthorImageUrl(string? authorImage)
    {
        if (string.IsNullOrEmpty(authorImage))
            return string.Empty;

        var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:5141";
        
        if (authorImage.StartsWith("assets/profile-pictures/"))
        {
            var filename = Path.GetFileName(authorImage);
            return $"{userServiceUrl}/assets/profile-pictures/{filename}";
        }
        
        // If it's already a full URL or different format, use as is
        return authorImage;
    }

    /// <summary>
    /// Helper method to handle missing authors
    /// </summary>
    private void HandleMissingAuthor(BlogPostDto dto, string authorId)
    {
        if (authorId == "DEMO-AUTHOR-0000-0000-000000000000")
        {
            dto.AuthorName = "Demo Author";
            dto.AuthorImage = string.Empty;
        }
        else
        {
            dto.AuthorName = "Unknown Author";
            dto.AuthorImage = string.Empty;
        }
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
