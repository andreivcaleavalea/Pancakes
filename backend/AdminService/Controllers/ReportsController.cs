using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportsService reportsService,
            ILogger<ReportsController> logger)
        {
            _reportsService = reportsService;
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
            var result = await _reportsService.GetReportsAsync(page, pageSize, status);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            _logger.LogError("Error getting reports: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> GetReport(string id)
        {
            var result = await _reportsService.GetReportByIdAsync(id);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            _logger.LogError("Error getting report {ReportId}: {Errors}", id, string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageReports")]
        public async Task<IActionResult> UpdateReport(string id, [FromBody] object updateData)
        {
            var currentAdminId = GetCurrentAdminId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();
            
            var result = await _reportsService.UpdateReportAsync(id, updateData, currentAdminId, ipAddress, userAgent);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message
                });
            }

            _logger.LogError("Error updating report {ReportId}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageReports")]
        public async Task<IActionResult> DeleteReport(string id)
        {
            var currentAdminId = GetCurrentAdminId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();
            
            var result = await _reportsService.DeleteReportAsync(id, currentAdminId, ipAddress, userAgent);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message
                });
            }

            _logger.LogError("Error deleting report {ReportId}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpGet("stats")]
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> GetReportStats()
        {
            var result = await _reportsService.GetReportStatsAsync();
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            _logger.LogError("Error getting report statistics: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }
    }
}
