using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BlogService.Services.Interfaces;

namespace BlogService.Controllers;

[ApiController]
[Route("")]
[AllowAnonymous]
public class AssetsController : ControllerBase
{
    private readonly IBlogImageService _blogImageService;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IBlogImageService blogImageService, ILogger<AssetsController> logger)
    {
        _blogImageService = blogImageService;
        _logger = logger;
    }

    [HttpGet("assets/blog-images/{filename}")]
    public async Task<IActionResult> GetBlogImage(string filename)
    {
        try
        {
            if (string.IsNullOrEmpty(filename))
            {
                return BadRequest("Filename is required");
            }

            // Validate filename to prevent directory traversal
            if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
            {
                return BadRequest("Invalid filename");
            }

            // Construct the file path
            var filePath = Path.Combine("assets", "blog-images", filename);

            // Serve the file directly
            var fileResult = await _blogImageService.GetBlogImageAsync(filePath);
            if (fileResult == null)
            {
                return NotFound("Blog image not found");
            }

            var (fileBytes, contentType, fileName) = fileResult.Value;
            
            // Add cache headers for better performance
            Response.Headers["Cache-Control"] = "public, max-age=86400"; // Cache for 24 hours (longer than profile pictures)
            Response.Headers["ETag"] = $"\"{Convert.ToHexString(System.Security.Cryptography.MD5.HashData(fileBytes))}\"";
            
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blog image: {Filename}", filename);
            return StatusCode(500, "An error occurred while retrieving the blog image");
        }
    }
}

