using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IAuditService _auditService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            IUserManagementService userManagementService,
            IAuditService auditService,
            ILogger<UserManagementController> logger)
        {
            _userManagementService = userManagementService;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] UserSearchRequest request)
        {
            try
            {
                var users = await _userManagementService.SearchUsersAsync(request);
                return Ok(new ApiResponse<PagedResponse<UserOverviewDto>>
                {
                    Success = true,
                    Data = users,
                    Message = "Users retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while searching users"
                });
            }
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserDetail(string userId)
        {
            try
            {
                var user = await _userManagementService.GetUserDetailAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                return Ok(new ApiResponse<UserDetailDto>
                {
                    Success = true,
                    Data = user,
                    Message = "User details retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for {UserId}", userId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user details"
                });
            }
        }

        [HttpPost("ban")]
        [Authorize(Roles = "SuperAdmin,SystemAdmin,ContentModerator")]
        public async Task<IActionResult> BanUser([FromBody] BanUserRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var success = await _userManagementService.BanUserAsync(request, currentAdminId);
                
                if (success)
                {
                    await _auditService.LogActionAsync(currentAdminId, "USER_BANNED", "User", request.UserId,
                        request, GetClientIpAddress(), GetUserAgent());

                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "User banned successfully"
                    });
                }

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to ban user"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user {UserId}", request.UserId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while banning user"
                });
            }
        }

        [HttpPost("unban")]
        [Authorize(Roles = "SuperAdmin,SystemAdmin,ContentModerator")]
        public async Task<IActionResult> UnbanUser([FromBody] UnbanUserRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var success = await _userManagementService.UnbanUserAsync(request, currentAdminId);
                
                if (success)
                {
                    await _auditService.LogActionAsync(currentAdminId, "USER_UNBANNED", "User", request.UserId,
                        request, GetClientIpAddress(), GetUserAgent());

                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "User unbanned successfully"
                    });
                }

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to unban user"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user {UserId}", request.UserId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while unbanning user"
                });
            }
        }

        [HttpPut("users/{userId}")]
        [Authorize(Roles = "SuperAdmin,SystemAdmin")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var user = await _userManagementService.UpdateUserAsync(userId, request, currentAdminId);
                
                if (user != null)
                {
                    await _auditService.LogActionAsync(currentAdminId, "USER_UPDATED", "User", userId,
                        request, GetClientIpAddress(), GetUserAgent());

                    return Ok(new ApiResponse<UserOverviewDto>
                    {
                        Success = true,
                        Data = user,
                        Message = "User updated successfully"
                    });
                }

                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating user"
                });
            }
        }

        [HttpPost("force-password-reset")]
        [Authorize(Roles = "SuperAdmin,SystemAdmin")]
        public async Task<IActionResult> ForcePasswordReset([FromBody] ForcePasswordResetRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var success = await _userManagementService.ForcePasswordResetAsync(request, currentAdminId);
                
                await _auditService.LogActionAsync(currentAdminId, "FORCE_PASSWORD_RESET", "User", request.UserId,
                    request, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password reset initiated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forcing password reset for user {UserId}", request.UserId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while initiating password reset"
                });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                var stats = await _userManagementService.GetUserStatisticsAsync();
                return Ok(new ApiResponse<Dictionary<string, object>>
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