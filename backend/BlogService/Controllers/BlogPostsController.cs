using BlogService.Models.DTOs;
using BlogService.Models.Requests;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostsController : ControllerBase
{
    private readonly IBlogPostService _blogPostService;
    private readonly ILogger<BlogPostsController> _logger;
    private readonly IFriendsPostService _friendsPostService;

    public BlogPostsController(
        IBlogPostService blogPostService, 
        ILogger<BlogPostsController> logger,
        IFriendsPostService friendsPostService)
    {
        _blogPostService = blogPostService;
        _logger = logger;
        _friendsPostService = friendsPostService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetBlogPosts([FromQuery] BlogPostQueryParameters parameters)
    {
        try
        {
            var result = await _blogPostService.GetAllAsync(parameters, HttpContext);
            
            // 🚀 HTTP CACHE: Cache blog posts for 5 minutes
            Response.Headers["Cache-Control"] = "public, max-age=300"; // 5 minutes
            Response.Headers["Vary"] = "Accept-Encoding, Authorization";
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog posts");
            return StatusCode(500, "An error occurred while getting blog posts");
        }
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
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
    [AllowAnonymous]
    public async Task<IActionResult> GetFeaturedPosts([FromQuery] int count = 5)
    {
        try
        {
            var posts = await _blogPostService.GetFeaturedAsync(count);
            
            // 🚀 HTTP CACHE: Cache featured posts for 15 minutes
            Response.Headers["Cache-Control"] = "public, max-age=900"; // 15 minutes
            Response.Headers["Vary"] = "Accept-Encoding";
            
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured posts");
            return StatusCode(500, "An error occurred while getting featured posts");
        }
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPopularPosts([FromQuery] int count = 5)
    {
        try
        {
            // Use personalized recommendations for authenticated users
            var posts = await _blogPostService.GetPersonalizedPopularAsync(count, HttpContext);
            
            // 🚀 HTTP CACHE: Cache popular posts for 5 minutes (shorter due to personalization)
            Response.Headers["Cache-Control"] = "public, max-age=300"; // 5 minutes
            Response.Headers["Vary"] = "Accept-Encoding, Authorization";
            
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular posts");
            return StatusCode(500, "An error occurred while getting popular posts");
        }
    }

    [HttpPost("{id}/view")]
    [AllowAnonymous]
    public async Task<IActionResult> IncrementViewCount(Guid id)
    {
        try
        {
            await _blogPostService.IncrementViewCountAsync(id);
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
            var blogPost = await _blogPostService.CreateAsync(createDto, HttpContext);
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
            var blogPost = await _blogPostService.UpdateAsync(id, updateDto, HttpContext);
            return Ok(blogPost);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update blog post {Id}", id);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog post with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the blog post");
        }
    }

    [HttpGet("drafts")]
    public async Task<IActionResult> GetDrafts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _blogPostService.GetUserDraftsAsync(HttpContext, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user drafts");
            return StatusCode(500, "An error occurred while retrieving drafts");
        }
    }

    [HttpPatch("{id:guid}/convert-to-draft")]
    public async Task<IActionResult> ConvertToDraft(Guid id)
    {
        try
        {
            var blogPost = await _blogPostService.ConvertToDraftAsync(id, HttpContext);
            return Ok(blogPost);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to convert blog post {Id} to draft", id);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting blog post {Id} to draft", id);
            return StatusCode(500, "An error occurred while converting the blog post to draft");
        }
    }

    [HttpPatch("{id:guid}/publish")]
    public async Task<IActionResult> PublishDraft(Guid id)
    {
        try
        {
            var blogPost = await _blogPostService.PublishDraftAsync(id, HttpContext);
            return Ok(blogPost);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to publish blog post {Id}", id);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing blog post {Id}", id);
            return StatusCode(500, "An error occurred while publishing the blog post");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBlogPost(Guid id)
    {
        try
        {
            await _blogPostService.DeleteAsync(id, HttpContext);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to delete blog post {Id}", id);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog post with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the blog post");
        }
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriendsPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _friendsPostService.GetFriendsPostsAsync(HttpContext, page, pageSize);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving friends' posts");
            return StatusCode(500, "An error occurred while retrieving friends' posts");
        }
    }
}
