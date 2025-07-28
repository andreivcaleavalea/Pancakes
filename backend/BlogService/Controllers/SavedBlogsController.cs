using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SavedBlogsController : ControllerBase
{
    private readonly ISavedBlogService _savedBlogService;
    private readonly ILogger<SavedBlogsController> _logger;
    private readonly IAuthorizationService _authorizationService;

    public SavedBlogsController(
        ISavedBlogService savedBlogService,
        ILogger<SavedBlogsController> logger,
        IAuthorizationService authorizationService)
    {
        _savedBlogService = savedBlogService;
        _logger = logger;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSavedBlogs()
    {
        try
        {
            // Get current user using authorization service
            var currentUser = await _authorizationService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return Unauthorized("Authorization token is required or invalid");
            }

            var savedBlogs = await _savedBlogService.GetSavedBlogsByUserIdAsync(currentUser.Id);
            return Ok(savedBlogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting saved blogs");
            return StatusCode(500, "An error occurred while getting saved blogs");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveBlog([FromBody] CreateSavedBlogDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current user using authorization service
            var currentUser = await _authorizationService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return Unauthorized("Authorization token is required or invalid");
            }

            var savedBlog = await _savedBlogService.SaveBlogAsync(currentUser.Id, createDto);
            return CreatedAtAction(nameof(GetSavedBlogs), savedBlog);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving blog");
            return StatusCode(500, "An error occurred while saving the blog");
        }
    }

    [HttpDelete("{blogPostId:guid}")]
    public async Task<IActionResult> UnsaveBlog(Guid blogPostId)
    {
        try
        {
            // Get current user using authorization service
            var currentUser = await _authorizationService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return Unauthorized("Authorization token is required or invalid");
            }

            await _savedBlogService.UnsaveBlogAsync(currentUser.Id, blogPostId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsaving blog");
            return StatusCode(500, "An error occurred while unsaving the blog");
        }
    }

    [HttpGet("check/{blogPostId:guid}")]
    public async Task<IActionResult> IsBookmarked(Guid blogPostId)
    {
        try
        {
            // Get current user using authorization service
            var currentUser = await _authorizationService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return Unauthorized("Authorization token is required or invalid");
            }

            var isBookmarked = await _savedBlogService.IsBookmarkedAsync(currentUser.Id, blogPostId);
            return Ok(new { isBookmarked });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking bookmark status");
            return StatusCode(500, "An error occurred while checking bookmark status");
        }
    }
}