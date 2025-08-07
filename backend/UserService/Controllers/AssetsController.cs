using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Route("")]
[AllowAnonymous]
public class AssetsController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IFileService fileService, ILogger<AssetsController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpGet("assets/profile-pictures/{filename}")]
    public async Task<IActionResult> GetProfilePicture(string filename)
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
            var filePath = Path.Combine("assets", "profile-pictures", filename);

            // Serve the file directly
            var fileResult = await _fileService.GetProfilePictureAsync(filePath);
            if (fileResult == null)
            {
                return NotFound("Profile picture not found");
            }

            var (fileBytes, contentType, fileName) = fileResult.Value;
            
            // Add cache headers for better performance
            Response.Headers["Cache-Control"] = "public, max-age=3600"; // Cache for 1 hour
            
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile picture: {Filename}", filename);
            return StatusCode(500, "An error occurred while retrieving the profile picture");
        }
    }
}