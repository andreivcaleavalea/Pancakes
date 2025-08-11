using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BlogService.Services.Interfaces;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using System.Security.Claims;

namespace BlogService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize] // Admin access will be handled by middleware/authorization
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetReports(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] ReportStatus? status = null)
    {
        try
        {
            IEnumerable<ReportDto> reports;
            
            if (status.HasValue)
            {
                reports = await _reportService.GetByStatusAsync(status.Value, page, pageSize);
            }
            else
            {
                reports = await _reportService.GetAllAsync(page, pageSize);
            }

            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports");
            return StatusCode(500, "An error occurred while retrieving reports");
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ReportDto>> GetReport(Guid id)
    {
        try
        {
            var report = await _reportService.GetByIdAsync(id);
            
            if (report == null)
            {
                return NotFound("Report not found");
            }

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report {ReportId}", id);
            return StatusCode(500, "An error occurred while retrieving the report");
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReportDto>> CreateReport(CreateReportDto createReportDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            // Check if user can report this content
            var (canReport, errorMessage) = await _reportService.ValidateReportAsync(
                userId, createReportDto.ContentId, createReportDto.ContentType);
            
            if (!canReport)
            {
                return BadRequest(new { message = errorMessage });
            }

            var report = await _reportService.CreateReportAsync(createReportDto, userId, userName);
            
            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report");
            return StatusCode(500, "An error occurred while creating the report");
        }
    }

    [HttpPut("{id}")]
    [Authorize] // Admin access will be handled by middleware/authorization
    public async Task<ActionResult<ReportDto>> UpdateReport(Guid id, UpdateReportDto updateReportDto)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown Admin";

            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized("Admin ID not found in token");
            }

            var report = await _reportService.UpdateReportAsync(id, updateReportDto, adminName);
            
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report {ReportId}", id);
            return StatusCode(500, "An error occurred while updating the report");
        }
    }

    [HttpDelete("{id}")]
    [Authorize] // Admin access will be handled by middleware/authorization
    public async Task<IActionResult> DeleteReport(Guid id)
    {
        try
        {
            await _reportService.DeleteReportAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report {ReportId}", id);
            return StatusCode(500, "An error occurred while deleting the report");
        }
    }

    [HttpGet("stats")]
    [Authorize] // Admin access will be handled by middleware/authorization
    public async Task<ActionResult<object>> GetReportStats()
    {
        try
        {
            var totalCount = await _reportService.GetTotalCountAsync();
            var pendingCount = await _reportService.GetPendingCountAsync();

            return Ok(new
            {
                TotalReports = totalCount,
                PendingReports = pendingCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report statistics");
            return StatusCode(500, "An error occurred while retrieving report statistics");
        }
    }

    [HttpGet("my-reports")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetMyReports()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var reports = await _reportService.GetByReporterIdAsync(userId);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user reports");
            return StatusCode(500, "An error occurred while retrieving your reports");
        }
    }

    [HttpGet("can-report/{contentType}/{contentId}")]
    [Authorize]
    public async Task<ActionResult<bool>> CanReportContent(ReportContentType contentType, Guid contentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var canReport = await _reportService.CanUserReportContentAsync(userId, contentId, contentType);
            return Ok(canReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user can report content");
            return StatusCode(500, "An error occurred while checking report permissions");
        }
    }
}
