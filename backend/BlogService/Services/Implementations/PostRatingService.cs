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

    public PostRatingService(
        IPostRatingRepository ratingRepository,
        IBlogPostRepository blogPostRepository,
        IMapper mapper)
    {
        _ratingRepository = ratingRepository;
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
    }

    public async Task<PostRatingStatsDto> GetRatingStatsAsync(Guid blogPostId, string? userIdentifier = null)
    {
        var averageRating = await _ratingRepository.GetAverageRatingAsync(blogPostId);
        var totalRatings = await _ratingRepository.GetTotalRatingsAsync(blogPostId);
        var ratingDistribution = await _ratingRepository.GetRatingDistributionAsync(blogPostId);
        
        decimal? userRating = null;
        if (!string.IsNullOrEmpty(userIdentifier))
        {
            var userRatingEntity = await _ratingRepository.GetByBlogPostAndUserAsync(blogPostId, userIdentifier);
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
        var existingRating = await _ratingRepository.GetByBlogPostAndUserAsync(createDto.BlogPostId, createDto.UserIdentifier);
        
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

    public async Task DeleteRatingAsync(Guid blogPostId, string userIdentifier)
    {
        var existingRating = await _ratingRepository.GetByBlogPostAndUserAsync(blogPostId, userIdentifier);
        if (existingRating == null)
        {
            throw new ArgumentException("Rating not found for this user and blog post.");
        }

        await _ratingRepository.DeleteAsync(existingRating.Id);
    }
} 