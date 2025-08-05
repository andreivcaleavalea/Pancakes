using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

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

    public CommentService(
        ICommentRepository commentRepository,
        IBlogPostRepository blogPostRepository,
        IMapper mapper,
        IUserServiceClient userServiceClient,
        IAuthorizationService authorizationService,
        IModelValidationService modelValidationService,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
        _userServiceClient = userServiceClient;
        _authorizationService = authorizationService;
        _modelValidationService = modelValidationService;
        _logger = logger;
    }

    public async Task<CommentDto?> GetByIdAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null) return null;
        
        var commentDto = _mapper.Map<CommentDto>(comment);
        await PopulateAuthorInfoAsync(commentDto);
        return commentDto;
    }

    public async Task<IEnumerable<CommentDto>> GetByBlogPostIdAsync(Guid blogPostId)
    {
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
        
        // Populate author information for all comments and replies
        foreach (var commentDto in commentDtos)
        {
            await PopulateAuthorInfoRecursivelyAsync(commentDto);
        }
        
        // Log AuthorIds after population
        foreach (var dto in commentDtos)
        {
            _logger.LogInformation("Comment {CommentId} after populate - AuthorId: {AuthorId}, AuthorName: {AuthorName}", 
                dto.Id, dto.AuthorId, dto.AuthorName);
        }
        
        return commentDtos;
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto createDto)
    {
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
        return commentDto;
    }

    public async Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto)
    {
        var existingComment = await GetCommentByIdOrThrowAsync(id);
        
        // Only allow updating content - preserve author information
        existingComment.Content = updateDto.Content;
        
        var updatedComment = await _commentRepository.UpdateAsync(existingComment);
        var commentDto = _mapper.Map<CommentDto>(updatedComment);
        await PopulateAuthorInfoAsync(commentDto);
        return commentDto;
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetCommentByIdOrThrowAsync(id);
        await _commentRepository.DeleteAsync(id);
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

    // New HttpContext-aware methods that handle all business logic
    public async Task<CommentDto> CreateAsync(CreateCommentDto createDto, HttpContext httpContext, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        _logger.LogInformation("Creating comment for blog post: {BlogPostId}", createDto?.BlogPostId);

        // Validate model state
        var validationResult = _modelValidationService.ValidateModel(modelState);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Model state invalid: {Errors}", validationResult.ErrorMessage);
            throw new ArgumentException(validationResult.ErrorMessage);
        }

        // Get current user using authorization service
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required");
        }

        _logger.LogInformation("Current user retrieved: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        // Override AuthorId with current user's ID to ensure security
        createDto.AuthorId = currentUser.Id;
        createDto.AuthorName = currentUser.Name;
        _logger.LogInformation("Updated createDto.AuthorId to: {AuthorId}, AuthorName to: {AuthorName}", 
            createDto.AuthorId, createDto.AuthorName);

        var result = await CreateAsync(createDto);
        _logger.LogInformation("Comment created with final AuthorId: {AuthorId}, AuthorName: {AuthorName}", 
            result.AuthorId, result.AuthorName);
        
        return result;
    }

    public async Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto, HttpContext httpContext, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        _logger.LogInformation("Updating comment with ID: {CommentId}", id);

        // Validate model state
        var validationResult = _modelValidationService.ValidateModel(modelState);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(validationResult.ErrorMessage);
        }

        // Get current user using authorization service
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required");
        }

        _logger.LogInformation("Current user retrieved for comment update: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        // Get the comment to check ownership
        var existingComment = await GetCommentByIdOrThrowAsync(id);
        
        // Check if the current user is the author of the comment
        if (existingComment.AuthorId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You can only update your own comments.");
        }
        
        return await UpdateAsync(id, updateDto);
    }

    public async Task<CommentDto?> DeleteAsync(Guid id, HttpContext httpContext)
    {
        _logger.LogInformation("Deleting comment with ID: {CommentId}", id);

        // Get current user using authorization service
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required");
        }

        _logger.LogInformation("Current user retrieved for comment delete: Id={UserId}, Name={UserName}", 
            currentUser.Id, currentUser.Name);

        // Get the comment to check ownership
        var existingComment = await GetCommentByIdOrThrowAsync(id);
        
        // Check if the current user is the author of the comment
        if (existingComment.AuthorId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You can only delete your own comments.");
        }
        
        return await SoftDeleteAsync(id);
    }

    private async Task<CommentDto?> SoftDeleteAsync(Guid id)
    {
        var comment = await GetCommentByIdOrThrowAsync(id);
        
        // Check if comment has replies
        var hasReplies = await _commentRepository.HasRepliesAsync(id);
        
        if (hasReplies)
        {
            // Soft delete: mark as deleted and change content
            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
            comment.Content = "[deleted]"; // Special marker for frontend
            comment.UpdatedAt = DateTime.UtcNow;
            
            await _commentRepository.UpdateAsync(comment);
            
            // Get the updated comment with all its replies
            var commentWithReplies = await _commentRepository.GetByIdWithRepliesAsync(id);
            if (commentWithReplies == null)
            {
                throw new InvalidOperationException($"Comment {id} not found after update");
            }
            
            var commentDto = _mapper.Map<CommentDto>(commentWithReplies);
            // Populate author info for the deleted comment and all its replies
            await PopulateAuthorInfoRecursivelyAsync(commentDto);
            
            _logger.LogInformation("Comment {CommentId} soft deleted due to existing replies", id);
            return commentDto;
        }
        else
        {
            // Hard delete if no replies
            await _commentRepository.DeleteAsync(id);
            _logger.LogInformation("Comment {CommentId} hard deleted (no replies)", id);
            return null; // Indicates complete deletion
        }
    }
} 