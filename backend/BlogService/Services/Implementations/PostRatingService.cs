using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class PostRatingService : IPostRatingService
{
    private readonly IPostRatingRepository _ratingRepository;
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;
    private readonly IModelValidationService _modelValidationService;
    private readonly ILogger<PostRatingService> _logger;

    public PostRatingService(
        IPostRatingRepository ratingRepository,
        IBlogPostRepository blogPostRepository,
        IMapper mapper,
        IUserContextService userContextService,
        IModelValidationService modelValidationService,
        ILogger<PostRatingService> logger)
    {
        _ratingRepository = ratingRepository;
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
        _userContextService = userContextService;
        _modelValidationService = modelValidationService;
        _logger = logger;
    }

    public async Task<PostRatingStatsDto> GetRatingStatsAsync(Guid blogPostId, string? UserId = null)
    {
        var averageRating = await _ratingRepository.GetAverageRatingAsync(blogPostId);
        var totalRatings = await _ratingRepository.GetTotalRatingsAsync(blogPostId);
        var ratingDistribution = await _ratingRepository.GetRatingDistributionAsync(blogPostId);
        
        decimal? userRating = null;
        if (!string.IsNullOrEmpty(UserId))
        {
            var userRatingEntity = await _ratingRepository.GetByBlogPostAndUserAsync(blogPostId, UserId);
            userRating = userRatingEntity?.Rating;
        }

        return new PostRatingStatsDto
        {
            BlogPostId = blogPostId,
            AverageRating = Math.Round(averageRating, 1),
            TotalRatings = totalRatings,
            UserRating = userRating,
            RatingDistribution = ratingDistribution
        };
    }

    public async Task<PostRatingDto> CreateOrUpdateRatingAsync(CreatePostRatingDto createDto)
    {
        // Verify blog post exists
        var blogPostExists = await _blogPostRepository.ExistsAsync(createDto.BlogPostId);
        if (!blogPostExists)
        {
            throw new ArgumentException($"Blog post with ID {createDto.BlogPostId} not found.");
        }

        // Validate rating value (must be between 0.5 and 5.0)
        if (createDto.Rating < 0.5m || createDto.Rating > 5.0m)
        {
            throw new ArgumentException("Rating must be between 0.5 and 5.0");
        }

        // Round to nearest 0.5 to ensure consistency
        createDto.Rating = Math.Round(createDto.Rating * 2) / 2;

        // Check if user already rated this post
        var existingRating = await _ratingRepository.GetByBlogPostAndUserAsync(createDto.BlogPostId, createDto.UserId);
        
        PostRating rating;
        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Rating = createDto.Rating;
            rating = await _ratingRepository.UpdateAsync(existingRating);
        }
        else
        {
            // Create new rating
            rating = _mapper.Map<PostRating>(createDto);
            rating = await _ratingRepository.CreateAsync(rating);
        }

        return _mapper.Map<PostRatingDto>(rating);
    }

    public async Task DeleteRatingAsync(Guid blogPostId, string UserId)
    {
        var existingRating = await _ratingRepository.GetByBlogPostAndUserAsync(blogPostId, UserId);
        if (existingRating == null)
        {
            throw new ArgumentException("Rating not found for this user and blog post.");
        }

        await _ratingRepository.DeleteAsync(existingRating.Id);
    }

    // New HttpContext-aware methods that handle all business logic
    public async Task<PostRatingStatsDto> GetRatingStatsAsync(Guid blogPostId, HttpContext httpContext)
    {
        // Allow both authenticated and unauthenticated access to rating stats
        var userId = _userContextService.GetCurrentUserId(httpContext);
        
        _logger.LogInformation("Getting rating stats for blog post {BlogPostId}, user {UserId}", 
            blogPostId, userId ?? "anonymous");

        return await GetRatingStatsAsync(blogPostId, userId);
    }

    public async Task<PostRatingDto> CreateOrUpdateRatingAsync(CreatePostRatingDto createDto, HttpContext httpContext, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        if (createDto == null)
        {
            _logger.LogWarning("Received null createDto");
            throw new ArgumentException("Request body is required");
        }

        _logger.LogInformation("Received rating request: BlogPostId={BlogPostId}, Rating={Rating}, UserAgent={UserAgent}", 
            createDto.BlogPostId, createDto.Rating, httpContext.Request.Headers.UserAgent.ToString());

        // Validate model state
        var validationResult = _modelValidationService.ValidateModel(modelState);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Model state invalid: {Errors}", validationResult.ErrorMessage);
            throw new ArgumentException(validationResult.ErrorMessage);
        }

        // Set user identifier using context service
        _logger.LogInformation("Attempting to get user ID...");
        createDto.UserId = _userContextService.GetCurrentUserId(httpContext);
        _logger.LogInformation("Successfully extracted user ID: {UserId}", createDto.UserId);

        return await CreateOrUpdateRatingAsync(createDto);
    }

    public async Task DeleteRatingAsync(Guid blogPostId, HttpContext httpContext)
    {
        var userId = _userContextService.GetCurrentUserId(httpContext);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("User {UserId} deleting rating for blog post {BlogPostId}", userId, blogPostId);
        await DeleteRatingAsync(blogPostId, userId);
    }
} 