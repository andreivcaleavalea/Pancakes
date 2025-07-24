using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentsController> _logger;
    private readonly IJwtUserService _jwtUserService;
    private readonly IUserServiceClient _userServiceClient;

    public CommentsController(
        ICommentService commentService, 
        ILogger<CommentsController> logger,
        IJwtUserService jwtUserService,
        IUserServiceClient userServiceClient)
    {
        _commentService = commentService;
        _logger = logger;
        _jwtUserService = jwtUserService;
        _userServiceClient = userServiceClient;
    }

    [HttpGet("blog/{blogPostId:guid}")]
    public async Task<IActionResult> GetCommentsByBlogPostId(Guid blogPostId)
    {
        try
        {
            var comments = await _commentService.GetByBlogPostIdAsync(blogPostId);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for blog post {BlogPostId}", blogPostId);
            return StatusCode(500, "An error occurred while getting comments");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetComment(Guid id)
    {
        try
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment with ID {CommentId}", id);
            return StatusCode(500, "An error occurred while getting the comment");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto createDto)
    {
        try
        {
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
            createDto.AuthorName = currentUser.Name;
            _logger.LogInformation("Updated createDto.AuthorId to: {AuthorId}", createDto.AuthorId);

            var comment = await _commentService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment");
            return StatusCode(500, "An error occurred while creating the comment");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] CreateCommentDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _commentService.UpdateAsync(id, updateDto);
            return Ok(comment);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment with ID {CommentId}", id);
            return StatusCode(500, "An error occurred while updating the comment");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        try
        {
            await _commentService.DeleteAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment with ID {CommentId}", id);
            return StatusCode(500, "An error occurred while deleting the comment");
        }
    }
} 