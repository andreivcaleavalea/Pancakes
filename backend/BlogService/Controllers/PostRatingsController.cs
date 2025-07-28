using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostRatingsController : ControllerBase
{
    private readonly IPostRatingService _ratingService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<PostRatingsController> _logger;

    public PostRatingsController(
        IPostRatingService ratingService, 
        IUserContextService userContextService,
        ILogger<PostRatingsController> logger)
    {
        _ratingService = ratingService;
        _userContextService = userContextService;
        _logger = logger;
    }

    [HttpGet("stats/{blogPostId:guid}")]
    public async Task<IActionResult> GetRatingStats(Guid blogPostId)
    {
        try
        {
            // Allow both authenticated and unauthenticated access to rating stats
            var userId = _userContextService.GetCurrentUserId(HttpContext);

            var stats = await _ratingService.GetRatingStatsAsync(blogPostId, userId);
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

            // Set user identifier using context service
            _logger.LogInformation("Attempting to get user ID...");
            createDto.UserId = _userContextService.GetCurrentUserId(HttpContext);
            _logger.LogInformation("Successfully extracted user ID: {UserId}", createDto.UserId);

            var rating = await _ratingService.CreateOrUpdateRatingAsync(createDto);
            return Ok(rating);
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
            var userId = _userContextService.GetCurrentUserId(HttpContext);
            await _ratingService.DeleteRatingAsync(blogPostId, userId);
            return NoContent();
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
} 