using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Clients.UserClient.DTOs;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserManagementController(
        IUserManagementService userManagementService,
        ILogger<UserManagementController> logger)
        : ControllerBase
    {
        [HttpGet("search")]
        [Authorize(Policy = "CanViewUsers")]
        public async Task<IActionResult> SearchUsers([FromQuery] UserSearchRequest request)
        {
            var result = await userManagementService.SearchUsersAsync(request);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<PagedResponse<UserOverviewDto>>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            logger.LogError("Error searching users: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpGet("users/{userId}")]
        [Authorize(Policy = "CanViewUserDetails")]
        public async Task<IActionResult> GetUserDetail(string userId)
        {
            var result = await userManagementService.GetUserDetailAsync(userId);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<UserDetailDto>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            logger.LogError("Error getting user details for {UserId}: {Errors}", userId, string.Join(", ", result.Errors));
            
            // Check if it's a not found vs server error
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpPost("ban")]
        [Authorize(Policy = "CanBanUsers")]
        public async Task<IActionResult> BanUser([FromBody] BanUserRequest request)
        {
            var currentAdminId = GetCurrentAdminId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();
            
            var result = await userManagementService.BanUserAsync(request, currentAdminId, ipAddress, userAgent);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message
                });
            }

            logger.LogError("Error banning user {UserId}: {Errors}", request.UserId, string.Join(", ", result.Errors));
            
            // Check if it's a validation error vs server error
            if (result.Errors.Any(e => e.Contains("Validation failed", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message,
                    Errors = result.Errors.ToList()
                });
            }

            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpPost("unban")]
        [Authorize(Policy = "CanUnbanUsers")]
        public async Task<IActionResult> UnbanUser([FromBody] UnbanUserRequest request)
        {
            var currentAdminId = GetCurrentAdminId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();
            
            var result = await userManagementService.UnbanUserAsync(request, currentAdminId, ipAddress, userAgent);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message
                });
            }

            logger.LogError("Error unbanning user {UserId}: {Errors}", request.UserId, string.Join(", ", result.Errors));
            
            // Check if it's a validation error vs server error
            if (result.Errors.Any(e => e.Contains("Validation failed", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message,
                    Errors = result.Errors.ToList()
                });
            }

            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpPut("users/{userId}")]
        [Authorize(Policy = "CanUpdateUsers")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            var currentAdminId = GetCurrentAdminId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();
            
            var result = await userManagementService.UpdateUserAsync(userId, request, currentAdminId, ipAddress, userAgent);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<UserDetailDto>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            logger.LogError("Error updating user {UserId}: {Errors}", userId, string.Join(", ", result.Errors));
            
            // Check if it's a not found vs server error
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpPost("force-password-reset")]
        [Authorize(Policy = "CanUpdateUsers")]
        public async Task<IActionResult> ForcePasswordReset([FromBody] ForcePasswordResetRequest request)
        {
            var currentAdminId = GetCurrentAdminId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();
            
            var result = await userManagementService.ForcePasswordResetAsync(request, currentAdminId, ipAddress, userAgent);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message
                });
            }

            logger.LogError("Error forcing password reset for user {UserId}: {Errors}", request.UserId, string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpGet("statistics")]
        [Authorize(Policy = "CanViewAnalytics")]
        public async Task<IActionResult> GetUserStatistics()
        {
            var result = await userManagementService.GetUserStatisticsAsync();
            
            if (result.Success)
            {
                return Ok(new ApiResponse<Dictionary<string, object>>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            logger.LogError("Error getting user statistics: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
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