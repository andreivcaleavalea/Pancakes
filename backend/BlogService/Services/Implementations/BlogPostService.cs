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
    private readonly IRecommendationService _recommendationService;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IModelValidationService _modelValidationService;
    private readonly IJwtUserService _jwtUserService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BlogPostService> _logger;

    public BlogPostService(
        IBlogPostRepository blogPostRepository,
        IUserServiceClient userServiceClient,
        IRecommendationService recommendationService,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IModelValidationService modelValidationService,
        IJwtUserService jwtUserService,
        IMemoryCache cache,
        ILogger<BlogPostService> logger)
    {
        _blogPostRepository = blogPostRepository;
        _userServiceClient = userServiceClient;
        _recommendationService = recommendationService;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _modelValidationService = modelValidationService;
        _jwtUserService = jwtUserService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BlogPostDto?> GetByIdAsync(Guid id)
    {
        var blogPost = await _blogPostRepository.GetByIdAsync(id);
        if (blogPost == null) return null;
        
        // Filter out deleted posts for regular users (only show published posts)
        if (blogPost.Status == PostStatus.Deleted)
        {
            return null;
        }
        
        var blogPostDto = _mapper.Map<BlogPostDto>(blogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        
        return blogPostDto;
    }

    public async Task<PaginatedResult<BlogPostDto>> GetAllAsync(BlogPostQueryParameters parameters)
    {
        // 🚀 CACHE: Cache paginated results based on parameters
        var paramHash = CacheConfig.CreateHash(
            parameters.Page, 
            parameters.PageSize, 
            parameters.SortBy ?? "CreatedAt",
            parameters.SortOrder ?? "desc",
            parameters.Status?.ToString() ?? "Published",
            parameters.Search ?? "",
            string.Join(",", parameters.Tags ?? new List<string>()),
            parameters.AuthorId ?? "",
            parameters.ExcludeAuthorId ?? "",
            parameters.DateFrom?.ToString() ?? "",
            parameters.DateTo?.ToString() ?? ""
        );
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, parameters.Page, parameters.PageSize, paramHash);
        
        if (_cache.TryGetValue(cacheKey, out PaginatedResult<BlogPostDto>? cachedResult) && cachedResult != null)
        {
            _logger.LogInformation("🎯 [Cache Hit] Paginated posts returned from cache for page: {Page}, size: {PageSize}", parameters.Page, parameters.PageSize);
            return cachedResult;
        }

        _logger.LogInformation("🔄 [Cache Miss] Loading paginated posts from database for page: {Page}, size: {PageSize}", parameters.Page, parameters.PageSize);
        var (posts, totalCount) = await _blogPostRepository.GetAllAsync(parameters);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // ✅ OPTIMIZED: Batch populate author information for all posts
        await PopulateAuthorInfoBatchAsync(blogPostDtos);
        
        var result = PaginationHelper.CreatePaginatedResult(blogPostDtos, parameters.Page, parameters.PageSize, totalCount);
        
        // 🚀 CACHE: Store with short duration (paginated results change more frequently)
        _cache.Set(cacheKey, result, CacheConfig.Duration.Short);
        _logger.LogInformation("✅ [Cache Set] Paginated posts cached for {Duration} minutes", CacheConfig.Duration.Short.TotalMinutes);
        
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
        // 🚀 CACHE: Featured posts are perfect for caching (rarely change, frequently accessed)
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.FeaturedPosts, count);
        if (_cache.TryGetValue(cacheKey, out IEnumerable<BlogPostDto>? cachedPosts) && cachedPosts != null)
        {
            _logger.LogInformation("🎯 [Cache Hit] Featured posts returned from cache for count: {Count}", count);
            return cachedPosts;
        }

        _logger.LogInformation("🔄 [Cache Miss] Loading featured posts from database for count: {Count}", count);
        var posts = await _blogPostRepository.GetFeaturedAsync(count);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);

        // ✅ OPTIMIZED: Batch populate author information
        await PopulateAuthorInfoBatchAsync(blogPostDtos);

        // 🚀 CACHE: Store with medium duration (featured posts don't change often)
        _cache.Set(cacheKey, blogPostDtos, CacheConfig.Duration.Medium);
        _logger.LogInformation("✅ [Cache Set] Featured posts cached for {Duration} minutes", CacheConfig.Duration.Medium.TotalMinutes);

        return blogPostDtos;
    }

    public async Task<IEnumerable<BlogPostDto>> GetPopularAsync(int count = 5)
    {
        // 🚀 CACHE: Popular posts for fallback scenarios
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.PopularPosts, count);
        if (_cache.TryGetValue(cacheKey, out IEnumerable<BlogPostDto>? cachedPosts) && cachedPosts != null)
        {
            _logger.LogInformation("🎯 [Cache Hit] Popular posts returned from cache for count: {Count}", count);
            return cachedPosts;
        }

        _logger.LogInformation("📈 [BlogPostService] GetPopularAsync called (FALLBACK - no personalization) with count: {Count}", count);
        
        var posts = await _blogPostRepository.GetPopularAsync(count);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        _logger.LogInformation("📊 [BlogPostService] Regular popular posts returned: {Count} posts", blogPostDtos.Count());
        _logger.LogInformation("📝 [BlogPostService] Popular post titles: {Titles}", 
            string.Join(", ", blogPostDtos.Select(p => p.Title)));
        
        // ✅ OPTIMIZED: Batch populate author information for all posts
        await PopulateAuthorInfoBatchAsync(blogPostDtos);
        
        // 🚀 CACHE: Store with short duration (popular posts change more frequently)
        _cache.Set(cacheKey, blogPostDtos, CacheConfig.Duration.Short);
        _logger.LogInformation("✅ [Cache Set] Popular posts cached for {Duration} minutes", CacheConfig.Duration.Short.TotalMinutes);
        
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
        
        // Notify friends if this is a published blog post
        if (createdBlogPost.Status == PostStatus.Published)
        {
            await NotifyFriendsAboutNewBlogPostAsync(currentUser, blogPostDto, httpContext);
        }
        
        // 🚀 CACHE INVALIDATION: Clear caches so new blog appears immediately
        InvalidateBlogPostCaches(currentUser.Id);
        
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
        
        _logger.LogInformation("Blog post {BlogPostId} updated successfully", id);
        
        var blogPostDto = _mapper.Map<BlogPostDto>(updatedBlogPost);
        await PopulateAuthorInfoAsync(blogPostDto);
        
        // 🚀 CACHE INVALIDATION: Clear caches so updated blog reflects immediately
        InvalidateBlogPostCaches(blogPostToUpdate.AuthorId);
        
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
        
        // 🚀 CACHE INVALIDATION: Clear caches so deleted blog disappears immediately
        InvalidateBlogPostCaches(blogPost.AuthorId);
        
        _logger.LogInformation("Blog post {BlogPostId} deleted successfully", id);
    }

    /// <summary>
    /// 🚀 CACHE INVALIDATION: Clear relevant cache entries when blog posts are modified
    /// This ensures users see their content immediately after create/update/delete operations
    /// </summary>
    private void InvalidateBlogPostCaches(string? authorId = null)
    {
        try
        {
            var keysToRemove = new List<string>();
            
            _logger.LogInformation("🧹 [Cache Invalidation] Clearing blog post caches for author: {AuthorId}", authorId ?? "all");

            // Clear featured posts cache (all common variants)
            for (int i = 1; i <= 10; i++)
            {
                keysToRemove.Add(CacheConfig.FormatKey(CacheConfig.Keys.FeaturedPosts, i));
            }

            // Clear popular posts cache (all common variants)
            for (int i = 1; i <= 10; i++)
            {
                keysToRemove.Add(CacheConfig.FormatKey(CacheConfig.Keys.PopularPosts, i));
            }

            // Clear paginated posts cache for common page/size combinations
            var commonPageSizes = new[] { 6, 9, 10, 12, 15, 20 };
            var commonPages = Enumerable.Range(1, 5); // Clear first 5 pages

            foreach (var pageSize in commonPageSizes)
            {
                foreach (var page in commonPages)
                {
                    // Clear multiple parameter combinations that are commonly used
                    var hashes = new[]
                    {
                        CacheConfig.CreateHash(page, pageSize, "CreatedAt", "desc", "Published", "", "", "", "", "", ""),
                        CacheConfig.CreateHash(page, pageSize, "UpdatedAt", "desc", "Published", "", "", "", "", "", ""),
                        CacheConfig.CreateHash(page, pageSize, "PublishedAt", "desc", "Published", "", "", "", "", "", "")
                    };

                    foreach (var hash in hashes)
                    {
                        keysToRemove.Add(CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, page, pageSize, hash));
                    }
                }
            }

            // Remove all cache keys
            foreach (var key in keysToRemove.Distinct())
            {
                _cache.Remove(key);
            }

            _logger.LogInformation("✅ [Cache Invalidation] Removed {CacheKeyCount} cache entries", keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [Cache Invalidation] Error clearing cache entries");
            // Don't throw - cache invalidation failure shouldn't break the main operation
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
        catch (Exception)
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

    public async Task<PaginatedResult<BlogPostDto>> GetUserDraftsAsync(HttpContext httpContext, int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("🔄 [GetUserDraftsAsync] Getting user drafts: Page={Page}, PageSize={PageSize}", page, pageSize);

        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        _logger.LogInformation("👤 [GetUserDraftsAsync] Current user retrieved for drafts: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        // Create query parameters to get user's draft posts
        var queryParams = new BlogPostQueryParameters
        {
            AuthorId = currentUser.Id,
            Status = PostStatus.Draft,
            Page = page,
            PageSize = pageSize,
            SortBy = "UpdatedAt",
            SortOrder = "desc"
        };

        var result = await GetAllAsync(queryParams);

        var firstDraft = result.Data.FirstOrDefault();
        _logger.LogInformation("📊 [GetUserDraftsAsync] Retrieved {Count} drafts out of {TotalCount} total drafts for user {UserId}. First draft: Title={FirstTitle}, UpdatedAt={UpdatedAt}", 
            result.Data.Count(), result.Pagination.TotalItems, currentUser.Id, 
            firstDraft?.Title ?? "None", firstDraft?.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A");

        return result;
    }

    public async Task<BlogPostDto> ConvertToDraftAsync(Guid id, HttpContext httpContext)
    {
        _logger.LogInformation("Converting blog post to draft: ID={BlogPostId}", id);

        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        var blogPost = await GetBlogPostByIdOrThrowAsync(id);

        // Check if user owns the post (or add admin check if needed)
        if (blogPost.AuthorId != currentUser.Id)
        {
            // You can add admin role check here if admins should be able to convert any post to draft
            throw new UnauthorizedAccessException("You don't have permission to modify this blog post");
        }

        blogPost.Status = PostStatus.Draft;
        blogPost.UpdatedAt = DateTime.UtcNow;
        blogPost.PublishedAt = null; // Clear published date when converting to draft

        await _blogPostRepository.UpdateAsync(blogPost);

        var blogPostDto = _mapper.Map<BlogPostDto>(blogPost);
        await PopulateAuthorInfoAsync(blogPostDto);

        // 🚀 CACHE INVALIDATION: Clear caches when post is converted to draft
        InvalidateBlogPostCaches(currentUser.Id);

        _logger.LogInformation("Blog post converted to draft successfully: ID={BlogPostId}", id);
        return blogPostDto;
    }

    public async Task<BlogPostDto> PublishDraftAsync(Guid id, HttpContext httpContext)
    {
        _logger.LogInformation("Publishing draft blog post: ID={BlogPostId}", id);

        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        var blogPost = await GetBlogPostByIdOrThrowAsync(id);

        // Check if user owns the post
        if (blogPost.AuthorId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this blog post");
        }

        // Check if it's actually a draft
        if (blogPost.Status != PostStatus.Draft)
        {
            throw new ArgumentException("Blog post is not a draft");
        }

        blogPost.Status = PostStatus.Published;
        blogPost.UpdatedAt = DateTime.UtcNow;
        blogPost.PublishedAt = DateTime.UtcNow;

        await _blogPostRepository.UpdateAsync(blogPost);

        var blogPostDto = _mapper.Map<BlogPostDto>(blogPost);
        await PopulateAuthorInfoAsync(blogPostDto);

        // Notify friends about the newly published blog post
        await NotifyFriendsAboutNewBlogPostAsync(currentUser, blogPostDto, httpContext);

        // 🚀 CACHE INVALIDATION: Clear caches when draft is published
        InvalidateBlogPostCaches(currentUser.Id);

        _logger.LogInformation("Draft blog post published successfully: ID={BlogPostId}", id);
        return blogPostDto;
    }

    /// <summary>
    /// Notify friends when a new blog post is published
    /// </summary>
    private async Task NotifyFriendsAboutNewBlogPostAsync(UserInfoDto author, BlogPostDto blogPost, HttpContext httpContext)
    {
        try
        {
            // Get the auth token from the HTTP context
            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            string? token = null;
            
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No auth token available for friend notifications for blog post {BlogPostId}", blogPost.Id);
                return;
            }

            // Get the author's friends
            var friends = await _userServiceClient.GetUserFriendsAsync(token);
            
            if (!friends.Any())
            {
                _logger.LogInformation("No friends to notify for blog post {BlogPostId} by {AuthorId}", blogPost.Id, author.Id);
                return;
            }

            // Create notifications for all friends
            var notificationTasks = friends.Select(async friend =>
            {
                try
                {
                    var notificationCreated = await _userServiceClient.CreateNotificationAsync(
                        userId: friend.UserId,
                        type: "FRIEND_BLOG_POSTED",
                        title: "Friend Posted a New Blog",
                        message: $"{author.Name} published a new blog post: \"{blogPost.Title}\"",
                        reason: "Friend blog post notification",
                        source: "BLOG_SYSTEM",
                        blogTitle: blogPost.Title,
                        blogId: blogPost.Id.ToString(),
                        authToken: token
                    );

                    if (notificationCreated)
                    {
                        _logger.LogInformation("Friend blog notification sent to {FriendId} for blog {BlogPostId}", friend.UserId, blogPost.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send friend blog notification to {FriendId} for blog {BlogPostId}", friend.UserId, blogPost.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending friend blog notification to {FriendId} for blog {BlogPostId}", friend.UserId, blogPost.Id);
                }
            });

            // Execute all notification tasks concurrently
            await Task.WhenAll(notificationTasks);
            
            _logger.LogInformation("Friend blog notifications processed for {FriendCount} friends for blog post {BlogPostId}", friends.Count(), blogPost.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying friends about new blog post {BlogPostId}", blogPost.Id);
            // Don't fail the whole operation if friend notifications fail
        }
    }

    public async Task<IEnumerable<BlogPostDto>> GetPersonalizedPopularAsync(int count = 3, HttpContext? httpContext = null)
    {
        try
        {
            _logger.LogInformation("🎯 [BlogPostService] GetPersonalizedPopularAsync called with count: {Count}", count);

            // If no context provided or user not authenticated, fallback to regular popular
            if (httpContext == null)
            {
                _logger.LogInformation("⚠️ [BlogPostService] No HttpContext provided - falling back to regular popular posts");
                return await GetPopularAsync(count);
            }

            // Log authentication details
            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            var hasAuthHeader = !string.IsNullOrEmpty(authHeader);
            _logger.LogInformation("🔐 [BlogPostService] Auth check - HasAuthHeader: {HasAuthHeader}, AuthHeaderLength: {Length}", 
                hasAuthHeader, authHeader?.Length ?? 0);

            var userId = _jwtUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("👤 [BlogPostService] No user ID found - falling back to regular popular posts. HasAuthHeader: {HasAuthHeader}", hasAuthHeader);
                return await GetPopularAsync(count);
            }

            _logger.LogInformation("✅ [BlogPostService] Authenticated user found: {UserId} - using personalized recommendations", userId);

            // Use recommendation service for authenticated users
            var recommendedPosts = await _recommendationService.GetPersonalizedRecommendationsAsync(
                userId, count, userId);

            _logger.LogInformation("📊 [BlogPostService] Personalized recommendations returned: {Count} posts for user {UserId}", 
                recommendedPosts.Count(), userId);

            // Convert to DTOs
            var recommendedDtos = new List<BlogPostDto>();
            foreach (var post in recommendedPosts)
            {
                var dto = _mapper.Map<BlogPostDto>(post);
                
                // Get user details for the author
                try
                {
                    var userDto = await _userServiceClient.GetUserByIdAsync(post.AuthorId);
                    if (userDto != null)
                    {
                        dto.AuthorName = userDto.Name;
                        dto.AuthorImage = userDto.Image;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get user details for author {AuthorId}", post.AuthorId);
                    dto.AuthorName = "Unknown Author";
                    dto.AuthorImage = string.Empty;
                }
                
                recommendedDtos.Add(dto);
            }

            return recommendedDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personalized popular posts");
            return await GetPopularAsync(count);
        }
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        await _blogPostRepository.IncrementViewCountAsync(id);
    }
}
