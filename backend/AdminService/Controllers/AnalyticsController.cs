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
        [Authorize(Policy = "CanViewDashboard")]
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

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedAnalytics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var detailedAnalytics = await _analyticsService.GetDetailedAnalyticsAsync(fromDate, toDate);
                
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
