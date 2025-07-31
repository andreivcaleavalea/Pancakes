using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminService.Services.Interfaces;
using AdminService.Models.Requests;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContentModerationController : ControllerBase
    {
        private readonly IContentModerationService _contentModerationService;
        private readonly ILogger<ContentModerationController> _logger;

        public ContentModerationController(
            IContentModerationService contentModerationService,
            ILogger<ContentModerationController> logger)
        {
            _contentModerationService = contentModerationService;
            _logger = logger;
        }

        [HttpGet("flags")]
        public async Task<IActionResult> GetContentFlags([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string status = "", [FromQuery] string contentType = "")
        {
            try
            {
                var flags = await _contentModerationService.GetContentFlagsAsync(page, pageSize, status, contentType);
                return Ok(new { success = true, data = flags, message = "Content flags retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content flags");
                return StatusCode(500, new { success = false, message = "Internal server error", errors = new[] { ex.Message } });
            }
        }

        [HttpGet("flags/pending")]
        public async Task<IActionResult> GetPendingFlags()
        {
            try
            {
                var flags = await _contentModerationService.GetPendingFlagsAsync();
                return Ok(new { success = true, data = flags, message = "Pending flags retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending flags");
                return StatusCode(500, new { success = false, message = "Internal server error", errors = new[] { ex.Message } });
            }
        }

        [HttpPost("flags/review")]
        public async Task<IActionResult> ReviewFlag([FromBody] ReviewFlagRequest request)
        {
            try
            {
                // Get current admin user from JWT token
                var currentAdminId = User.FindFirst("sub")?.Value ?? "unknown";
                var result = await _contentModerationService.ReviewContentFlagAsync(request, currentAdminId);
                if (result)
                {
                    return Ok(new { success = true, message = "Flag reviewed successfully" });
                }
                return BadRequest(new { success = false, message = "Failed to review flag" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing flag {FlagId}", request.FlagId);
                return StatusCode(500, new { success = false, message = "Internal server error", errors = new[] { ex.Message } });
            }
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetUserReports([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string status = "")
        {
            try
            {
                var reports = await _contentModerationService.GetUserReportsAsync(page, pageSize, status);
                return Ok(new { success = true, data = reports, message = "User reports retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user reports");
                return StatusCode(500, new { success = false, message = "Internal server error", errors = new[] { ex.Message } });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetModerationStatistics()
        {
            try
            {
                var stats = await _contentModerationService.GetModerationStatisticsAsync();
                return Ok(new { success = true, data = stats, message = "Moderation statistics retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving moderation statistics");
                return StatusCode(500, new { success = false, message = "Internal server error", errors = new[] { ex.Message } });
            }
        }
    }
}