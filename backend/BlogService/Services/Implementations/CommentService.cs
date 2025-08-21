using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using BlogService.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;

namespace BlogService.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IAuthorizationService _authorizationService;
    private readonly IModelValidationService _modelValidationService;
    private readonly ILogger<CommentService> _logger;
    private readonly IMemoryCache _cache;

    public CommentService(
        ICommentRepository commentRepository,
        IBlogPostRepository blogPostRepository,
        IMapper mapper,
        IUserServiceClient userServiceClient,
        IAuthorizationService authorizationService,
        IModelValidationService modelValidationService,
        ILogger<CommentService> logger,
        IMemoryCache cache)
    {
        _commentRepository = commentRepository;
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
        _userServiceClient = userServiceClient;
        _authorizationService = authorizationService;
        _modelValidationService = modelValidationService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<CommentDto?> GetByIdAsync(Guid id)
    {
        // 🚀 CACHE: Check cache first for individual comment
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.CommentById, id);
        if (_cache.TryGetValue(cacheKey, out CommentDto? cachedComment))
        {
            return cachedComment;
        }

        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null) return null;
        
        var commentDto = _mapper.Map<CommentDto>(comment);
        await PopulateAuthorInfoAsync(commentDto);
        
        // 🚀 CACHE: Store individual comment with medium duration
        _cache.Set(cacheKey, commentDto, CacheConfig.Duration.Medium);
        
        return commentDto;
    }

    public async Task<IEnumerable<CommentDto>> GetByBlogPostIdAsync(Guid blogPostId)
    {
        // 🚀 CACHE: Simple cache key for all comments by blog post (no pagination for this endpoint)
        var cacheKey = $"comments_all_by_post_{blogPostId}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<CommentDto>? cachedComments))
        {
            return cachedComments;
        }

        var comments = await _commentRepository.GetByBlogPostIdAsync(blogPostId);
        _logger.LogInformation("Retrieved {CommentCount} comments from repository for blog {BlogPostId}", 
            comments.Count(), blogPostId);
            
        var commentDtos = _mapper.Map<IEnumerable<CommentDto>>(comments);
        _logger.LogInformation("Mapped to {DtoCount} comment DTOs", commentDtos.Count());
        
        // Log AuthorIds before population
        foreach (var dto in commentDtos)
        {
            _logger.LogInformation("Comment {CommentId} before populate - AuthorId: {AuthorId}, AuthorName: {AuthorName}", 
                dto.Id, dto.AuthorId, dto.AuthorName);
        }
        
        // 🚀 OPTIMIZED: Batch populate author info to avoid N+1 queries
        await PopulateAuthorInfoBatchAsync(commentDtos);
        
        // Log AuthorIds after population
        foreach (var dto in commentDtos)
        {
            _logger.LogInformation("Comment {CommentId} after populate - AuthorId: {AuthorId}, AuthorName: {AuthorName}", 
                dto.Id, dto.AuthorId, dto.AuthorName);
        }
        
        // 🚀 CACHE: Store with short duration (comments can be added frequently)
        _cache.Set(cacheKey, commentDtos, CacheConfig.Duration.Short);
        
        return commentDtos;
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto createDto, HttpContext httpContext)
    {
        _logger.LogInformation("Creating comment for blog post: {BlogPostId}", createDto.BlogPostId);

        // Get current user from authentication context
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        _logger.LogInformation("Current user retrieved: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        // Set author information from authenticated user
        createDto.AuthorId = currentUser.Id;
        createDto.AuthorName = currentUser.Name;

        // Verify blog post exists
        var blogPostExists = await _blogPostRepository.ExistsAsync(createDto.BlogPostId);
        if (!blogPostExists)
        {
            throw new ArgumentException($"Blog post with ID {createDto.BlogPostId} not found.");
        }

        // If this is a reply, verify the parent comment exists
        if (createDto.ParentCommentId.HasValue)
        {
            var parentCommentExists = await _commentRepository.ExistsAsync(createDto.ParentCommentId.Value);
            if (!parentCommentExists)
            {
                throw new ArgumentException($"Parent comment with ID {createDto.ParentCommentId} not found.");
            }
        }

        _logger.LogInformation("Before mapping - createDto.AuthorId: {AuthorId}, createDto.AuthorName: {AuthorName}", 
            createDto.AuthorId, createDto.AuthorName);
            
        var comment = _mapper.Map<Comment>(createDto);
        _logger.LogInformation("After mapping - comment.AuthorId: {AuthorId}, comment.AuthorName: {AuthorName}", 
            comment.AuthorId, comment.AuthorName);
            
        var createdComment = await _commentRepository.CreateAsync(comment);
        _logger.LogInformation("After repository create - createdComment.AuthorId: {AuthorId}, createdComment.AuthorName: {AuthorName}", 
            createdComment.AuthorId, createdComment.AuthorName);
            
        var commentDto = _mapper.Map<CommentDto>(createdComment);
        _logger.LogInformation("Final DTO - commentDto.AuthorId: {AuthorId}, commentDto.AuthorName: {AuthorName}", 
            commentDto.AuthorId, commentDto.AuthorName);
        
        // Populate author info including image
        await PopulateAuthorInfoAsync(commentDto);
        
        // 🗑️ CACHE: Clear comment caches after creating new comment
        _logger.LogInformation("🔄 [CreateAsync] About to clear comment cache for blog post {BlogPostId}", createDto.BlogPostId);
        ClearCommentCaches(createDto.BlogPostId);
        
        return commentDto;
    }

    public async Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto, HttpContext httpContext)
    {
        // Get current user from authentication context
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        var existingComment = await GetCommentByIdOrThrowAsync(id);
        
        // Check if the current user is the author of the comment
        if (existingComment.AuthorId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You can only edit your own comments.");
        }
        
        // Only allow updating content - preserve author information
        existingComment.Content = updateDto.Content;
        
        var updatedComment = await _commentRepository.UpdateAsync(existingComment);
        var commentDto = _mapper.Map<CommentDto>(updatedComment);
        await PopulateAuthorInfoAsync(commentDto);
        
        // 🗑️ CACHE: Clear comment caches after updating comment
        _logger.LogInformation("🔄 [UpdateAsync] About to clear comment cache for blog post {BlogPostId}", existingComment.BlogPostId);
        ClearCommentCaches(existingComment.BlogPostId);
        
        return commentDto;
    }

    public async Task DeleteAsync(Guid id, HttpContext httpContext)
    {
        // Get current user from authentication context
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        var comment = await GetCommentByIdOrThrowAsync(id);
        
        // Check if the current user is the author of the comment
        if (comment.AuthorId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You can only delete your own comments.");
        }

        await _commentRepository.DeleteAsync(id);
        
        // 🗑️ CACHE: Clear comment caches after deleting comment
        _logger.LogInformation("🔄 [DeleteAsync] About to clear comment cache for blog post {BlogPostId}", comment.BlogPostId);
        ClearCommentCaches(comment.BlogPostId);
    }

    private async Task<Comment> GetCommentByIdOrThrowAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            throw new ArgumentException($"Comment with ID {id} not found.");
        }
        return comment;
    }

    /// <summary>
    /// 🗑️ Clear comment-related caches when comments are modified
    /// </summary>
    private void ClearCommentCaches(Guid blogPostId)
    {
        try
        {
            // Clear the simple comment cache key
            var cacheKey = $"comments_all_by_post_{blogPostId}";
            _cache.Remove(cacheKey);
            
            _logger.LogInformation("🗑️ [ClearCommentCaches] Removed cache key: {CacheKey} for blog post {BlogPostId}", cacheKey, blogPostId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear comment caches for blog post {BlogPostId}", blogPostId);
        }
    }
    
    private async Task PopulateAuthorInfoAsync(CommentDto commentDto)
    {
        _logger.LogInformation("PopulateAuthorInfoAsync - CommentId: {CommentId}, Input AuthorId: {AuthorId}, Input AuthorName: {AuthorName}", 
            commentDto.Id, commentDto.AuthorId, commentDto.AuthorName);
            
        if (string.IsNullOrEmpty(commentDto.AuthorId))
        {
            commentDto.AuthorName = "Anonymous";
            commentDto.AuthorImage = string.Empty;
            _logger.LogInformation("Set to Anonymous for comment {CommentId}", commentDto.Id);
            return;
        }
        
        try
        {
            _logger.LogInformation("Calling UserService for AuthorId: {AuthorId}", commentDto.AuthorId);
            var user = await _userServiceClient.GetUserByIdAsync(commentDto.AuthorId);
            
            if (user != null)
            {
                _logger.LogInformation("UserService returned - UserId: {UserId}, UserName: {UserName} for comment {CommentId}", 
                    user.Id, user.Name, commentDto.Id);
                    
                commentDto.AuthorName = user.Name;
                commentDto.AuthorImage = user.Image ?? string.Empty;
                
                // CRITICAL: Check if UserService is somehow returning different user data
                if (user.Id != commentDto.AuthorId)
                {
                    _logger.LogError("MISMATCH! UserService returned different user! Requested: {RequestedId}, Got: {ReturnedId} for comment {CommentId}", 
                        commentDto.AuthorId, user.Id, commentDto.Id);
                }
            }
            else
            {
                _logger.LogWarning("UserService returned null for AuthorId: {AuthorId} in comment {CommentId}", 
                    commentDto.AuthorId, commentDto.Id);
                commentDto.AuthorName = "Unknown User";
                commentDto.AuthorImage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService for AuthorId: {AuthorId} in comment {CommentId}", 
                commentDto.AuthorId, commentDto.Id);
                
            // If we can't get user info, keep the existing AuthorName or set a default
            if (string.IsNullOrEmpty(commentDto.AuthorName))
            {
                commentDto.AuthorName = "Unknown User";
            }
            commentDto.AuthorImage = string.Empty;
        }
        
        _logger.LogInformation("PopulateAuthorInfoAsync completed - CommentId: {CommentId}, Final AuthorId: {AuthorId}, Final AuthorName: {AuthorName}", 
            commentDto.Id, commentDto.AuthorId, commentDto.AuthorName);
    }
    
    /// <summary>
    /// 🚀 OPTIMIZED: Batch populate author info for all comments and replies to avoid N+1 queries
    /// </summary>
    private async Task PopulateAuthorInfoBatchAsync(IEnumerable<CommentDto> commentDtos)
    {
        var allComments = new List<CommentDto>();
        
        // Collect all comments and their replies recursively
        foreach (var comment in commentDtos)
        {
            CollectCommentsRecursively(comment, allComments);
        }
        
        if (!allComments.Any()) return;
        
        // Extract unique author IDs
        var authorIds = allComments
            .Select(c => c.AuthorId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();
            
        if (!authorIds.Any()) return;
        
        try
        {
            // 🚀 SINGLE batch API call instead of N individual calls
            var authors = await _userServiceClient.GetUsersByIdsAsync(authorIds, null);
            var authorLookup = authors.ToDictionary(a => a.Id, a => a);
            
            // Populate all comments with author info
            foreach (var comment in allComments)
            {
                if (string.IsNullOrEmpty(comment.AuthorId))
                {
                    comment.AuthorName = "Anonymous";
                    comment.AuthorImage = string.Empty;
                }
                else if (authorLookup.TryGetValue(comment.AuthorId, out var author))
                {
                    comment.AuthorName = author.Name;
                    comment.AuthorImage = author.Image ?? string.Empty;
                }
                else
                {
                    comment.AuthorName = "Unknown User";
                    comment.AuthorImage = string.Empty;
                }
            }
            
            _logger.LogInformation("✅ Batch populated author info for {CommentCount} comments using {AuthorCount} unique authors",
                allComments.Count, authorLookup.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch populating author info for comments");
            
            // Fallback: Set all to unknown
            foreach (var comment in allComments)
            {
                if (string.IsNullOrEmpty(comment.AuthorName))
                {
                    comment.AuthorName = "Unknown User";
                    comment.AuthorImage = string.Empty;
                }
            }
        }
    }
    
    /// <summary>
    /// Recursively collect all comments and their replies into a flat list
    /// </summary>
    private void CollectCommentsRecursively(CommentDto comment, List<CommentDto> allComments)
    {
        allComments.Add(comment);
        
        foreach (var reply in comment.Replies)
        {
            CollectCommentsRecursively(reply, allComments);
        }
    }

    private async Task PopulateAuthorInfoRecursivelyAsync(CommentDto commentDto)
    {
        // Populate author info for this comment
        await PopulateAuthorInfoAsync(commentDto);
        
        // Recursively populate author info for all replies
        foreach (var reply in commentDto.Replies)
        {
            await PopulateAuthorInfoRecursivelyAsync(reply);
        }
    }

} 