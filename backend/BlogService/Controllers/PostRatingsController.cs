using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostRatingsController : ControllerBase
{
    private readonly IPostRatingService _ratingService;
    private readonly ILogger<PostRatingsController> _logger;

    public PostRatingsController(IPostRatingService ratingService, ILogger<PostRatingsController> logger)
    {
        _ratingService = ratingService;
        _logger = logger;
    }

    [HttpGet("stats/{blogPostId:guid}")]
    public async Task<IActionResult> GetRatingStats(Guid blogPostId)
    {
        try
        {
            var userIdentifier = GetUserIdentifier();
            var stats = await _ratingService.GetRatingStatsAsync(blogPostId, userIdentifier);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rating stats for blog post {BlogPostId}", blogPostId);
            return StatusCode(500, "An error occurred while getting rating statistics");
        }
    }

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

            // Set user identifier from IP address
            createDto.UserIdentifier = GetUserIdentifier();

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
            var userIdentifier = GetUserIdentifier();
            await _ratingService.DeleteRatingAsync(blogPostId, userIdentifier);
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

    private string GetUserIdentifier()
    {
        // For now, use IP address as user identifier
        // In a real app, this would be user ID from authentication
        var userIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        // Create a simple hash of IP + UserAgent for better uniqueness
        var combined = $"{userIP}_{userAgent}";
        return combined.Length > 100 ? combined.Substring(0, 100) : combined;
    }
} 