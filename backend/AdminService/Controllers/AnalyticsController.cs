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
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var dashboardStats = await _analyticsService.GetDashboardStatsAsync();
                
                return Ok(new ApiResponse<DashboardStatsDto>
                {
                    Success = true,
                    Data = dashboardStats,
                    Message = "Dashboard statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving dashboard statistics"
                });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUserStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var userStats = await _analyticsService.GetUserStatsAsync(fromDate, toDate);
                
                return Ok(new ApiResponse<UserStatsDto>
                {
                    Success = true,
                    Data = userStats,
                    Message = "User statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user statistics"
                });
            }
        }

        [HttpGet("content")]
        public async Task<IActionResult> GetContentStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var contentStats = await _analyticsService.GetContentStatsAsync(fromDate, toDate);
                
                return Ok(new ApiResponse<ContentStatsDto>
                {
                    Success = true,
                    Data = contentStats,
                    Message = "Content statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving content statistics"
                });
            }
        }

        [HttpGet("moderation")]
        public async Task<IActionResult> GetModerationStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var moderationStats = await _analyticsService.GetModerationStatsAsync(fromDate, toDate);
                
                return Ok(new ApiResponse<ModerationStatsDto>
                {
                    Success = true,
                    Data = moderationStats,
                    Message = "Moderation statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving moderation statistics");
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
                var systemStats = await _analyticsService.GetSystemStatsAsync();
                
                return Ok(new ApiResponse<SystemStatsDto>
                {
                    Success = true,
                    Data = systemStats,
                    Message = "System statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system statistics");
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
                if (string.IsNullOrWhiteSpace(metricType))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Metric type is required"
                    });
                }

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
                _logger.LogError(ex, "Error retrieving daily metrics for {MetricType}", metricType);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving daily metrics"
                });
            }
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedAnalytics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var dashboardStats = await _analyticsService.GetDashboardStatsAsync();
                var userStats = await _analyticsService.GetUserStatsAsync(fromDate, toDate);
                var contentStats = await _analyticsService.GetContentStatsAsync(fromDate, toDate);
                var moderationStats = await _analyticsService.GetModerationStatsAsync(fromDate, toDate);
                var systemStats = await _analyticsService.GetSystemStatsAsync();

                var detailedAnalytics = new
                {
                    dashboard = dashboardStats,
                    users = userStats,
                    content = contentStats,
                    moderation = moderationStats,
                    system = systemStats
                };
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = detailedAnalytics,
                    Message = "Detailed analytics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving detailed analytics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving detailed analytics"
                });
            }
        }
    }
}
