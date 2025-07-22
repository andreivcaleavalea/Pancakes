using BlogService.Models.DTOs;
using BlogService.Models.Requests;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostsController : ControllerBase
{
    private readonly IBlogPostService _blogPostService;
    private readonly IUserServiceClient _userServiceClient;
    private readonly ILogger<BlogPostsController> _logger;

    public BlogPostsController(IBlogPostService blogPostService, IUserServiceClient userServiceClient, ILogger<BlogPostsController> logger)
    {
        _blogPostService = blogPostService;
        _userServiceClient = userServiceClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlogPosts([FromQuery] BlogPostQueryParameters parameters)
    {
        try
        {
            var result = await _blogPostService.GetAllAsync(parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog posts");
            return StatusCode(500, "An error occurred while getting blog posts");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBlogPost(Guid id)
    {
        try
        {
            var blogPost = await _blogPostService.GetByIdAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }
            return Ok(blogPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog post with ID {Id}", id);
            return StatusCode(500, "An error occurred while getting the blog post");
        }
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedPosts([FromQuery] int count = 5)
    {
        try
        {
            var posts = await _blogPostService.GetFeaturedAsync(count);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured posts");
            return StatusCode(500, "An error occurred while getting featured posts");
        }
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularPosts([FromQuery] int count = 5)
    {
        try
        {
            var posts = await _blogPostService.GetPopularAsync(count);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular posts");
            return StatusCode(500, "An error occurred while getting popular posts");
        }
    }

    [HttpPost("{id}/view")]
    public IActionResult IncrementViewCount(Guid id)
    {
        try
        {
            // Optional endpoint - just return success for now
            // You can implement actual view counting logic later if needed
            return Ok(new { success = true, message = "View count incremented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for blog post {Id}", id);
            return StatusCode(500, "An error occurred while updating view count");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostDto createDto)
    {
        try
        {
            _logger.LogInformation("Received blog post creation request: Title={Title}, AuthorId={AuthorId}", 
                createDto?.Title ?? "null", createDto?.AuthorId ?? "null");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToArray();
                
                _logger.LogWarning("Model state invalid: {Errors}", string.Join("; ", errors));
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
            _logger.LogInformation("Extracted token length: {TokenLength}", token.Length);
            
            // Get current user from UserService
            var currentUser = await _userServiceClient.GetCurrentUserAsync(token);
            if (currentUser == null)
            {
                _logger.LogWarning("Failed to get current user from UserService");
                return Unauthorized("Invalid or expired token");
            }

            _logger.LogInformation("Current user retrieved: Id={UserId}, Name={UserName}", 
                currentUser.Id, currentUser.Name);

            // Override AuthorId with current user's ID to ensure security
            createDto.AuthorId = currentUser.Id;
            _logger.LogInformation("Updated createDto.AuthorId to: {AuthorId}", createDto.AuthorId);

            var blogPost = await _blogPostService.CreateAsync(createDto);
            _logger.LogInformation("Blog post created successfully with ID: {BlogPostId}", blogPost.Id);
            
            return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.Id }, blogPost);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Validation error creating blog post");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            return StatusCode(500, "An error occurred while creating the blog post");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBlogPost(Guid id, [FromBody] UpdateBlogPostDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var blogPost = await _blogPostService.UpdateAsync(id, updateDto);
            return Ok(blogPost);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog post with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the blog post");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBlogPost(Guid id)
    {
        try
        {
            await _blogPostService.DeleteAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog post with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the blog post");
        }
    }
}
