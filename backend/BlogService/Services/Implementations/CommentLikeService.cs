using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class CommentLikeService : ICommentLikeService
{
    private readonly ICommentLikeRepository _likeRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;
    private readonly IModelValidationService _modelValidationService;

    public CommentLikeService(
        ICommentLikeRepository likeRepository,
        ICommentRepository commentRepository,
        IMapper mapper,
        IUserContextService userContextService,
        IModelValidationService modelValidationService)
    {
        _likeRepository = likeRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
        _userContextService = userContextService;
        _modelValidationService = modelValidationService;
    }

    public async Task<CommentLikeStatsDto> GetLikeStatsAsync(Guid commentId, string? UserId = null)
    {
        var likeCount = await _likeRepository.GetLikeCountAsync(commentId);
        var dislikeCount = await _likeRepository.GetDislikeCountAsync(commentId);
        
        bool? userLike = null;
        if (!string.IsNullOrEmpty(UserId))
        {
            var userLikeEntity = await _likeRepository.GetByCommentAndUserAsync(commentId, UserId);
            if (userLikeEntity != null)
            {
                userLike = userLikeEntity.IsLike;
            }
        }

        return new CommentLikeStatsDto
        {
            CommentId = commentId,
            LikeCount = likeCount,
            DislikeCount = dislikeCount,
            UserLike = userLike
        };
    }

    public async Task<CommentLikeDto> CreateOrUpdateLikeAsync(CreateCommentLikeDto createDto)
    {
        // Verify comment exists
        var commentExists = await _commentRepository.ExistsAsync(createDto.CommentId);
        if (!commentExists)
        {
            throw new ArgumentException($"Comment with ID {createDto.CommentId} not found.");
        }

        // Check if user already liked/disliked this comment
        var existingLike = await _likeRepository.GetByCommentAndUserAsync(createDto.CommentId, createDto.UserId);
        
        CommentLike like;
        if (existingLike != null)
        {
            // Update existing like/dislike
            existingLike.IsLike = createDto.IsLike;
            like = await _likeRepository.UpdateAsync(existingLike);
        }
        else
        {
            // Create new like/dislike
            like = _mapper.Map<CommentLike>(createDto);
            like = await _likeRepository.CreateAsync(like);
        }

        return _mapper.Map<CommentLikeDto>(like);
    }

    public async Task DeleteLikeAsync(Guid commentId, string UserId)
    {
        var existingLike = await _likeRepository.GetByCommentAndUserAsync(commentId, UserId);
        if (existingLike == null)
        {
            throw new ArgumentException("Like/dislike not found for this user and comment.");
        }

        await _likeRepository.DeleteAsync(existingLike.Id);
    }

    // New HttpContext-aware methods that handle all business logic
    public async Task<CommentLikeStatsDto> GetLikeStatsAsync(Guid commentId, HttpContext httpContext)
    {
        var userId = _userContextService.GetCurrentUserId(httpContext);
        return await GetLikeStatsAsync(commentId, userId);
    }

    public async Task<CommentLikeDto> CreateOrUpdateLikeAsync(CreateCommentLikeDto createDto, HttpContext httpContext, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        // Validate model state
        var validationResult = _modelValidationService.ValidateModel(modelState);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(validationResult.ErrorMessage);
        }

        // Set user identifier using context service
        createDto.UserId = _userContextService.GetCurrentUserId(httpContext);

        return await CreateOrUpdateLikeAsync(createDto);
    }

    public async Task DeleteLikeAsync(Guid commentId, HttpContext httpContext)
    {
        var userId = _userContextService.GetCurrentUserId(httpContext);
        await DeleteLikeAsync(commentId, userId);
    }
} 