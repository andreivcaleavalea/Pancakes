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
    private readonly IUserServiceClient _userServiceClient;

    public SavedBlogsController(
        ISavedBlogService savedBlogService,
        ILogger<SavedBlogsController> logger,
        IUserServiceClient userServiceClient)
    {
        _savedBlogService = savedBlogService;
        _logger = logger;
        _userServiceClient = userServiceClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetSavedBlogs()
    {
        try
        {
            // Extract the JWT token from Authorization header
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Missing or invalid authorization header");
                return Unauthorized("Authorization token is required");
            }

            var token = authHeader.Substring("Bearer ".Length);
            
            // Get current user from UserService
            var currentUser = await _userServiceClient.GetCurrentUserAsync(token);
            if (currentUser == null)
            {
                _logger.LogWarning("Failed to get current user from UserService");
                return Unauthorized("Invalid or expired token");
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

            // Extract the JWT token from Authorization header
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Missing or invalid authorization header");
                return Unauthorized("Authorization token is required");
            }

            var token = authHeader.Substring("Bearer ".Length);
            
            // Get current user from UserService
            var currentUser = await _userServiceClient.GetCurrentUserAsync(token);
            if (currentUser == null)
            {
                _logger.LogWarning("Failed to get current user from UserService");
                return Unauthorized("Invalid or expired token");
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
            // Extract the JWT token from Authorization header
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Missing or invalid authorization header");
                return Unauthorized("Authorization token is required");
            }

            var token = authHeader.Substring("Bearer ".Length);
            
            // Get current user from UserService
            var currentUser = await _userServiceClient.GetCurrentUserAsync(token);
            if (currentUser == null)
            {
                _logger.LogWarning("Failed to get current user from UserService");
                return Unauthorized("Invalid or expired token");
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
            // Extract the JWT token from Authorization header
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Missing or invalid authorization header");
                return Unauthorized("Authorization token is required");
            }

            var token = authHeader.Substring("Bearer ".Length);
            
            // Get current user from UserService
            var currentUser = await _userServiceClient.GetCurrentUserAsync(token);
            if (currentUser == null)
            {
                _logger.LogWarning("Failed to get current user from UserService");
                return Unauthorized("Invalid or expired token");
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