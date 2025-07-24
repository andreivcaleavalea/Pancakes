using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostRatingsController : ControllerBase
{
    private readonly IPostRatingService _ratingService;
    private readonly IJwtUserService _jwtUserService;
    private readonly ILogger<PostRatingsController> _logger;

    public PostRatingsController(
        IPostRatingService ratingService, 
        IJwtUserService jwtUserService,
        ILogger<PostRatingsController> logger)
    {
        _ratingService = ratingService;
        _jwtUserService = jwtUserService;
        _logger = logger;
    }

    [HttpGet("stats/{blogPostId:guid}")]
    public async Task<IActionResult> GetRatingStats(Guid blogPostId)
    {
        try
        {
            // Allow both authenticated and unauthenticated access to rating stats
            var UserId = _jwtUserService.GetCurrentUserId();

            var stats = await _ratingService.GetRatingStatsAsync(blogPostId, UserId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rating stats for blog post {BlogPostId}", blogPostId);
            return StatusCode(500, "An error occurred while getting rating statistics");
        }
    }

    [HttpPut]
    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateRating([FromBody] CreatePostRatingDto createDto)
    {
        try
        {
            if (createDto == null)
            {
                _logger.LogWarning("Received null createDto");
                return BadRequest("Request body is required");
            }

            _logger.LogInformation("Received rating request: BlogPostId={BlogPostId}, Rating={Rating}, UserAgent={UserAgent}", 
                createDto.BlogPostId, createDto.Rating, Request.Headers.UserAgent.ToString());

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToArray();
                
                _logger.LogWarning("Model state invalid: {Errors}", string.Join("; ", errors));
                return BadRequest(ModelState);
            }

            // Set user identifier from validated token
            _logger.LogInformation("Attempting to get user ID from JWT token...");
            try
            {
                createDto.UserId = GetUserId();
                _logger.LogInformation("Successfully extracted user ID: {UserId}", createDto.UserId);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed to extract user ID from JWT token: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            var rating = await _ratingService.CreateOrUpdateRatingAsync(createDto);
            return Ok(rating);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating rating");
            return StatusCode(500, "An error occurred while processing the rating");
        }
    }

    [HttpDelete("{blogPostId:guid}")]
    public async Task<IActionResult> DeleteRating(Guid blogPostId)
    {
        try
        {
            var UserId = GetUserId();
            await _ratingService.DeleteRatingAsync(blogPostId, UserId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rating for blog post {BlogPostId}", blogPostId);
            return StatusCode(500, "An error occurred while deleting the rating");
        }
    }

    private string GetUserId()
    {
        // Log the authorization header for debugging
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        _logger.LogInformation("Authorization header present: {HasAuth}, StartsWith Bearer: {IsBearer}", 
            !string.IsNullOrEmpty(authHeader), 
            authHeader?.StartsWith("Bearer ") == true);

        // Try to get user ID from JWT token first
        var userId = _jwtUserService.GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("JWT validation successful, user ID: {UserId}", userId);
            return userId;
        }

        // If no valid JWT token, throw unauthorized exception for rating operations
        _logger.LogWarning("JWT validation failed - no valid user ID found");
        throw new UnauthorizedAccessException("Authorization token is required for rating operations. Please log in to rate blog posts.");
    }
} 