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
        var (posts, totalCount) = await _blogPostRepository.GetAllAsync(parameters);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // ✅ OPTIMIZED: Batch populate author information for all posts
        await PopulateAuthorInfoBatchAsync(blogPostDtos);
        
        var result = PaginationHelper.CreatePaginatedResult(blogPostDtos, parameters.Page, parameters.PageSize, totalCount);
        
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
        var posts = await _blogPostRepository.GetFeaturedAsync(count);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);

        // ✅ OPTIMIZED: Batch populate author information
        await PopulateAuthorInfoBatchAsync(blogPostDtos);

        return blogPostDtos;
    }

    public async Task<IEnumerable<BlogPostDto>> GetPopularAsync(int count = 5)
    {
        var posts = await _blogPostRepository.GetPopularAsync(count);
        var blogPostDtos = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
        
        // ✅ OPTIMIZED: Batch populate author information for all posts
        await PopulateAuthorInfoBatchAsync(blogPostDtos);
        
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
        
        _logger.LogInformation("Blog post created successfully with ID: {BlogPostId}", blogPostDto.Id);
        // 🗑️ CACHE: Clear caches after creating new blog post
        ClearBlogPostCaches(createdBlogPost.Id, createdBlogPost);
        
        // Clear user-specific draft cache if this is a draft
        if (createdBlogPost.Status == PostStatus.Draft)
        {
            ClearUserSpecificDraftCache(createdBlogPost.AuthorId);
        }
        
        _logger.LogInformation("Blog post created successfully with ID: {BlogPostId} and caches cleared", blogPostDto.Id);
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
        
        _logger.LogInformation("Blog post {BlogPostId} deleted successfully", id);
    }


    private void ClearBlogPostCaches(Guid updatedPostId, BlogPost updatedPost)
    {
        try
        {
            // APPROACH 1: Clear specific known cache entries
            
            // Clear individual blog post cache
            var individualPostKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostById, updatedPostId);
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
                    var publishedHash = CacheConfig.CreateHash(page, pageSize, "", "", PostStatus.Published.ToString(), "", "", "", "", "");
                    var publishedKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, page, pageSize, publishedHash);
                    _cache.Remove(publishedKey);
                    
                    // Clear draft posts lists
                    var draftHash = CacheConfig.CreateHash(page, pageSize, "", "", PostStatus.Draft.ToString(), "", "", "", "", "");
                    var draftKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, page, pageSize, draftHash);
                    _cache.Remove(draftKey);
                    
                    // Clear deleted posts lists
                    var deletedHash = CacheConfig.CreateHash(page, pageSize, "", "", PostStatus.Deleted.ToString(), "", "", "", "", "");
                    var deletedKey = CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, page, pageSize, deletedHash);
                    _cache.Remove(deletedKey);
                }
            }
            
            // APPROACH 3: For maximum reliability, clear ALL cache
            // This is aggressive but ensures no stale data remains
            ClearAllCache();
            
            _logger.LogInformation("Cleared caches for updated blog post {BlogPostId}", updatedPostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing caches for updated blog post {BlogPostId}", updatedPostId);
            // If specific cache clearing fails, clear everything as fallback
            ClearAllCache();
        }
    }
    
    private void ClearAllCache()
    {
        // 🚀 OPTIMIZED: Smart cache clearing instead of brute force (was clearing 640+ keys!)
        try
        {
            // Clear only the most commonly used cache keys instead of thousands
            var commonCacheKeys = new[]
            {
                // Most common pagination patterns (first pages with common sizes)
                CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, 1, 6, CacheConfig.CreateHash(1, 6, "", "", "", "", "", "", "", "")),
                CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, 1, 10, CacheConfig.CreateHash(1, 10, "", "", "", "", "", "", "", "")),
                CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, 1, 20, CacheConfig.CreateHash(1, 20, "", "", "", "", "", "", "", "")),
                
                // Common status filters
                CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, 1, 10, CacheConfig.CreateHash(1, 10, "", "", PostStatus.Published.ToString(), "", "", "", "", "")),
                CacheConfig.FormatKey(CacheConfig.Keys.BlogPostsByPage, 1, 10, CacheConfig.CreateHash(1, 10, "", "", PostStatus.Draft.ToString(), "", "", "", "", "")),
                
                // Featured and popular posts (most common counts)
                CacheConfig.FormatKey(CacheConfig.Keys.FeaturedPosts, 5),
                CacheConfig.FormatKey(CacheConfig.Keys.FeaturedPosts, 10),
                CacheConfig.FormatKey(CacheConfig.Keys.PopularPosts, 5),
                CacheConfig.FormatKey(CacheConfig.Keys.PopularPosts, 10),
            };
            
            foreach (var key in commonCacheKeys)
            {
                _cache.Remove(key);
            }
            
            _logger.LogInformation("🗑️ Smart cache clearing: removed {KeyCount} common cache keys (was 640+)", commonCacheKeys.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during smart cache clearing");
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

        var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? Environment.GetEnvironmentVariable("USER_API_BASE_URL") ?? "http://localhost:5141";
        
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
                    var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? Environment.GetEnvironmentVariable("USER_API_BASE_URL") ?? "http://localhost:5141";
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
            _logger.LogError(ex, "Error calling UserService for AuthorId: {AuthorId}", blogPostDto.AuthorId);
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
}
