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

    public CommentLikesController(
        ICommentLikeService likeService, 
        ILogger<CommentLikesController> logger)
    {
        _likeService = likeService;
        _logger = logger;
    }

    [HttpGet("stats/{commentId:guid}")]
    public async Task<IActionResult> GetLikeStats(Guid commentId)
    {
        try
        {
            var stats = await _likeService.GetLikeStatsAsync(commentId, HttpContext);
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
            var like = await _likeService.CreateOrUpdateLikeAsync(createDto, HttpContext, ModelState);
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
            await _likeService.DeleteLikeAsync(commentId, HttpContext);
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
} 