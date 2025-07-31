using AdminService.Models.DTOs;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            IAuditService auditService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _analyticsService.GetDashboardStatsAsync();
                return Ok(new ApiResponse<DashboardStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Dashboard statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving dashboard statistics"
                });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUserStats([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var stats = await _analyticsService.GetUserStatsAsync(fromDate, toDate);
                return Ok(new ApiResponse<UserStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "User statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user statistics"
                });
            }
        }

        [HttpGet("content")]
        public async Task<IActionResult> GetContentStats([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var stats = await _analyticsService.GetContentStatsAsync(fromDate, toDate);
                return Ok(new ApiResponse<ContentStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Content statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving content statistics"
                });
            }
        }

        [HttpGet("moderation")]
        public async Task<IActionResult> GetModerationStats([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var stats = await _analyticsService.GetModerationStatsAsync(fromDate, toDate);
                return Ok(new ApiResponse<ModerationStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Moderation statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting moderation statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving moderation statistics"
                });
            }
        }

        [HttpGet("system")]
        public async Task<IActionResult> GetSystemStats()
        {
            try
            {
                var stats = await _analyticsService.GetSystemStatsAsync();
                return Ok(new ApiResponse<SystemStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "System statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving system statistics"
                });
            }
        }

        [HttpGet("metrics/daily")]
        public async Task<IActionResult> GetDailyMetrics([FromQuery] string metricType, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                var metrics = await _analyticsService.GetDailyMetricsAsync(metricType, fromDate, toDate);
                return Ok(new ApiResponse<List<DailyMetricDto>>
                {
                    Success = true,
                    Data = metrics,
                    Message = "Daily metrics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily metrics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving daily metrics"
                });
            }
        }

        [HttpPost("collect-metrics")]
        [Authorize(Roles = "SuperAdmin,SystemAdmin")]
        public async Task<IActionResult> CollectSystemMetrics()
        {
            try
            {
                await _analyticsService.CollectSystemMetricsAsync();
                
                var currentAdminId = GetCurrentAdminId();
                await _auditService.LogActionAsync(currentAdminId, "METRICS_COLLECTED", "SystemMetric", "", 
                    null, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "System metrics collected successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting system metrics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while collecting system metrics"
                });
            }
        }

        private string GetCurrentAdminId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }

        private string GetUserAgent()
        {
            return HttpContext.Request.Headers.UserAgent.ToString();
        }
    }
}