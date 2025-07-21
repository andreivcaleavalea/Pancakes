using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentLikesController : ControllerBase
{
    private readonly ICommentLikeService _likeService;
    private readonly ILogger<CommentLikesController> _logger;

    public CommentLikesController(ICommentLikeService likeService, ILogger<CommentLikesController> logger)
    {
        _likeService = likeService;
        _logger = logger;
    }

    [HttpGet("stats/{commentId:guid}")]
    public async Task<IActionResult> GetLikeStats(Guid commentId)
    {
        try
        {
            var userIdentifier = GetUserIdentifier();
            var stats = await _likeService.GetLikeStatsAsync(commentId, userIdentifier);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting like stats for comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while getting like statistics");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateLike([FromBody] CreateCommentLikeDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set user identifier from IP address
            createDto.UserIdentifier = GetUserIdentifier();

            var like = await _likeService.CreateOrUpdateLikeAsync(createDto);
            return Ok(like);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating like");
            return StatusCode(500, "An error occurred while processing the like");
        }
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteLike(Guid commentId)
    {
        try
        {
            var userIdentifier = GetUserIdentifier();
            await _likeService.DeleteLikeAsync(commentId, userIdentifier);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting like for comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while deleting the like");
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