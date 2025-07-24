using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentLikesController : ControllerBase
{
    private readonly ICommentLikeService _likeService;
    private readonly IJwtUserService _jwtUserService;
    private readonly ILogger<CommentLikesController> _logger;

    public CommentLikesController(
        ICommentLikeService likeService, 
        IJwtUserService jwtUserService,
        ILogger<CommentLikesController> logger)
    {
        _likeService = likeService;
        _jwtUserService = jwtUserService;
        _logger = logger;
    }

    [HttpGet("stats/{commentId:guid}")]
    public async Task<IActionResult> GetLikeStats(Guid commentId)
    {
        try
        {
            var UserId = GetUserId();
            var stats = await _likeService.GetLikeStatsAsync(commentId, UserId);
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
            createDto.UserId = GetUserId();

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
            var UserId = GetUserId();
            await _likeService.DeleteLikeAsync(commentId, UserId);
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

    private string GetUserId()
    {
        // Try to get user ID from JWT token first
        var userId = _jwtUserService.GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }

        // Fallback to IP address + UserAgent for anonymous users
        var userIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        // Create a simple hash of IP + UserAgent for better uniqueness
        var combined = $"anonymous_{userIP}_{userAgent}";
        return combined.Length > 100 ? combined.Substring(0, 100) : combined;
    }
} 