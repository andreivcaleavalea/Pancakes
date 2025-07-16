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
    private readonly ILogger<BlogPostsController> _logger;

    public BlogPostsController(IBlogPostService blogPostService, ILogger<BlogPostsController> logger)
    {
        _blogPostService = blogPostService;
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

    [HttpPost]
    public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var blogPost = await _blogPostService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.Id }, blogPost);
        }
        catch (ArgumentException ex)
        {
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
