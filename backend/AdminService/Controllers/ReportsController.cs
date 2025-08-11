using AdminService.Clients.BlogClient.Services;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly BlogServiceClient _blogServiceClient;
        private readonly IAuditService _auditService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            BlogServiceClient blogServiceClient,
            IAuditService auditService,
            ILogger<ReportsController> logger)
        {
            _blogServiceClient = blogServiceClient;
            _auditService = auditService;
            _logger = logger;
        }

        private string GetCurrentAdminId()
        {
            return User.FindFirst("sub")?.Value ?? 
                   User.FindFirst("id")?.Value ?? 
                   User.FindFirst("AdminId")?.Value ?? 
                   "unknown";
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private string GetUserAgent()
        {
            return Request.Headers["User-Agent"].ToString();
        }

        [HttpGet]
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> GetReports([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? status = null)
        {
            try
            {
                var reportsJson = await _blogServiceClient.GetReportsAsync(page, pageSize, status);
                
                // Parse and return the reports as JSON
                var reports = JsonSerializer.Deserialize<object>(reportsJson);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = reports,
                    Message = "Reports retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving reports"
                });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> GetReport(string id)
        {
            try
            {
                var reportJson = await _blogServiceClient.GetReportByIdAsync(id);
                var report = JsonSerializer.Deserialize<object>(reportJson);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = report,
                    Message = "Report retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report {ReportId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the report"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageReports")]
        public async Task<IActionResult> UpdateReport(string id, [FromBody] object updateData)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var success = await _blogServiceClient.UpdateReportAsync(id, updateData);

                if (!success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to update report"
                    });
                }

                await _auditService.LogActionAsync(currentAdminId, "REPORT_UPDATED", "Report", id,
                    updateData, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Report updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the report"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageReports")]
        public async Task<IActionResult> DeleteReport(string id)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var success = await _blogServiceClient.DeleteReportAsync(id);

                if (!success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to delete report"
                    });
                }

                await _auditService.LogActionAsync(currentAdminId, "REPORT_DELETED", "Report", id,
                    null, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Report deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the report"
                });
            }
        }

        [HttpGet("stats")]
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> GetReportStats()
        {
            try
            {
                var statsJson = await _blogServiceClient.GetReportStatsAsync();
                var stats = JsonSerializer.Deserialize<object>(statsJson);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = stats,
                    Message = "Report statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving report statistics"
                });
            }
        }
    }
}
