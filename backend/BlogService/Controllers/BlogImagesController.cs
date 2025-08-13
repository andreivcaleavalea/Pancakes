using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogService.Services.Interfaces;
using AuthorizationService = BlogService.Services.Interfaces.IAuthorizationService;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogImagesController : ControllerBase
{
    private readonly IBlogImageService _blogImageService;
    private readonly AuthorizationService _authorizationService;
    private readonly ILogger<BlogImagesController> _logger;

    public BlogImagesController(
        IBlogImageService blogImageService,
        AuthorizationService authorizationService,
        ILogger<BlogImagesController> logger)
    {
        _blogImageService = blogImageService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadBlogImage(IFormFile image)
    {
        try
        {
            if (image == null)
            {
                return BadRequest("No image file provided");
            }

            var currentUser = await _authorizationService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return Unauthorized("User not authenticated");
            }

            if (!_blogImageService.IsValidImageFile(image))
            {
                return BadRequest("Invalid image file. Supported formats: JPG, JPEG, PNG, GIF, WEBP. Maximum size: 10MB.");
            }

            var imagePath = await _blogImageService.SaveBlogImageAsync(image, currentUser.Id);
            
            // Return the URL that can be used to access the image
            var imageUrl = $"{Request.Scheme}://{Request.Host}/assets/blog-images/{Path.GetFileName(imagePath)}";
            
            return Ok(new { 
                imagePath = imagePath,
                imageUrl = imageUrl,
                message = "Image uploaded successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid image file provided for upload");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blog image");
            return StatusCode(500, "An error occurred while uploading the image");
        }
    }

    [HttpDelete("{filename}")]
    [Authorize]
    public async Task<IActionResult> DeleteBlogImage(string filename)
    {
        try
        {
            if (string.IsNullOrEmpty(filename))
            {
                return BadRequest("Filename is required");
            }

            var currentUser = await _authorizationService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return Unauthorized("User not authenticated");
            }

            // Validate filename to prevent directory traversal
            if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
            {
                return BadRequest("Invalid filename");
            }

            // Check if the user owns this image (filename should start with their userId)
            if (!filename.StartsWith(currentUser.Id))
            {
                return Forbid("You can only delete your own images");
            }

            var filePath = Path.Combine("assets", "blog-images", filename);
            await _blogImageService.DeleteBlogImageAsync(filePath);

            return Ok(new { message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog image: {Filename}", filename);
            return StatusCode(500, "An error occurred while deleting the image");
        }
    }
}
