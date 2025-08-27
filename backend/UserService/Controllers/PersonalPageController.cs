using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Models.DTOs;
using UserService.Models.Requests;
using UserService.Services.Interfaces;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonalPageController : ControllerBase
{
    private readonly IPersonalPageService _personalPageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PersonalPageController> _logger;

    public PersonalPageController(
        IPersonalPageService personalPageService,
        ICurrentUserService currentUserService,
        ILogger<PersonalPageController> logger)
    {
        _personalPageService = personalPageService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    // Get current user's personal page settings
    [HttpGet("settings")]
    [Authorize]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var settings = await _personalPageService.GetSettingsAsync(currentUserId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal page settings");
            return StatusCode(500, "An error occurred while getting personal page settings");
        }
    }

    // Update current user's personal page settings
    [HttpPut("settings")]
    [Authorize]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdatePersonalPageSettingsDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var updatedSettings = await _personalPageService.UpdateSettingsAsync(currentUserId, updateDto);
            return Ok(updatedSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating personal page settings");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal page settings");
            return StatusCode(500, "An error occurred while updating personal page settings");
        }
    }

    // Get public personal page by slug
    [HttpGet("public/{pageSlug}")]
    public async Task<IActionResult> GetPublicPage(string pageSlug)
    {
        try
        {
            var publicPage = await _personalPageService.GetPublicPageAsync(pageSlug);
            
            if (publicPage == null)
            {
                return NotFound("Personal page not found or not public");
            }

            return Ok(publicPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public personal page for slug {PageSlug}", pageSlug);
            return StatusCode(500, "An error occurred while getting the personal page");
        }
    }

    // Get paginated public portfolios
    [HttpGet("public-all")]
    public async Task<IActionResult> GetPublicPortfolios([FromQuery] PortfolioQueryParameters parameters)
    {
        try
        {
            var result = await _personalPageService.GetPublicPortfoliosAsync(parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public portfolios");
            return StatusCode(500, "An error occurred while getting public portfolios");
        }
    }

    // Generate unique slug suggestion
    [HttpPost("generate-slug")]
    [Authorize]
    public async Task<IActionResult> GenerateSlug([FromBody] GenerateSlugRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var slug = await _personalPageService.GenerateUniqueSlugAsync(request.BaseName, currentUserId);
            return Ok(new { slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating slug");
            return StatusCode(500, "An error occurred while generating slug");
        }
    }

    // Get current user's personal page data (for preview)
    [HttpGet("preview")]
    [Authorize]
    public async Task<IActionResult> GetPreview()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var settings = await _personalPageService.GetSettingsAsync(currentUserId);
            
            if (!string.IsNullOrEmpty(settings.PageSlug))
            {
                var publicPage = await _personalPageService.GetPublicPageAsync(settings.PageSlug);
                return Ok(publicPage);
            }

            return NotFound("Personal page not configured");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal page preview");
            return StatusCode(500, "An error occurred while getting the personal page preview");
        }
    }
}

public class GenerateSlugRequest
{
    public string BaseName { get; set; } = string.Empty;
} 