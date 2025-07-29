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

    public PostRatingsController(
        IPostRatingService ratingService, 
        ILogger<PostRatingsController> logger)
    {
        _ratingService = ratingService;
        _logger = logger;
    }

    [HttpGet("stats/{blogPostId:guid}")]
    public async Task<IActionResult> GetRatingStats(Guid blogPostId)
    {
        try
        {
            var stats = await _ratingService.GetRatingStatsAsync(blogPostId, HttpContext);
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
            var rating = await _ratingService.CreateOrUpdateRatingAsync(createDto, HttpContext, ModelState);
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
            await _ratingService.DeleteRatingAsync(blogPostId, HttpContext);
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
} 