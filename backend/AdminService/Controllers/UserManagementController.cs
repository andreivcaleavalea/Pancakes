using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Clients.UserClient;
using AdminService.Clients.UserClient.DTOs;
using AdminService.Services.Interfaces;
using AdminService.Validations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserManagementController(
        UserServiceClient userServiceClient,
        IAuditService auditService,
        ILogger<UserManagementController> logger)
        : ControllerBase
    {
        [HttpGet("search")]
        [Authorize(Policy = "CanViewUsers")]
        public async Task<IActionResult> SearchUsers([FromQuery] UserSearchRequest request)
        {
            logger.LogError("Starting search");
            try
            {
                var users = await userServiceClient.SearchUsersAsync(request);
                return Ok(new ApiResponse<PagedResponse<UserOverviewDto>>
                {
                    Success = true,
                    Data = users,
                    Message = "Users retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error searching users");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while searching users"
                });
            }
        }

        [HttpGet("users/{userId}")]
        [Authorize(Policy = "CanViewUserDetails")]
        public async Task<IActionResult> GetUserDetail(string userId)
        {
            try
            {
                var user = await userServiceClient.GetUserDetailAsync(userId);
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
                logger.LogError(ex, "Error getting user details for {UserId}", userId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user details"
                });
            }
        }

        [HttpPost("ban")]
        [Authorize(Policy = "CanBanUsers")]
        public async Task<IActionResult> BanUser([FromBody] BanUserRequest request)
        {
            try
            {
                var validationResult = BanRequestValidator.ValidateBanRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = validationResult.Errors.ToList()
                    });
                }

                var currentAdminId = GetCurrentAdminId();
                var success = await userServiceClient.BanUserAsync(request, currentAdminId);

                if (!success)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to ban user"
                    });
                await auditService.LogActionAsync(currentAdminId, "USER_BANNED", "User", request.UserId,
                    request, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User banned successfully"
                });

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error banning user {UserId}", request.UserId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while banning user"
                });
            }
        }

        [HttpPost("unban")]
        [Authorize(Policy = "CanUnbanUsers")]
        public async Task<IActionResult> UnbanUser([FromBody] UnbanUserRequest request)
        {
            try
            {
                var validationResult = UnbanRequestValidator.ValidateUnbanRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = validationResult.Errors.ToList()
                    });
                }

                var currentAdminId = GetCurrentAdminId();
                var success = await userServiceClient.UnbanUserAsync(request, currentAdminId);
                
                if (success)
                {
                    await auditService.LogActionAsync(currentAdminId, "USER_UNBANNED", "User", request.UserId,
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
                logger.LogError(ex, "Error unbanning user {UserId}", request.UserId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while unbanning user"
                });
            }
        }

        [HttpPut("users/{userId}")]
        [Authorize(Policy = "CanUpdateUsers")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var user = await userServiceClient.UpdateUserAsync(userId, request, currentAdminId);
                
                if (user != null)
                {
                    await auditService.LogActionAsync(currentAdminId, "USER_UPDATED", "User", userId,
                        request, GetClientIpAddress(), GetUserAgent());

                    return Ok(new ApiResponse<UserDetailDto>
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
                logger.LogError(ex, "Error updating user {UserId}", userId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating user"
                });
            }
        }

        [HttpPost("force-password-reset")]
        [Authorize(Policy = "CanUpdateUsers")]
        public async Task<IActionResult> ForcePasswordReset([FromBody] ForcePasswordResetRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var success = await userServiceClient.ForcePasswordResetAsync(request, currentAdminId);
                
                await auditService.LogActionAsync(currentAdminId, "FORCE_PASSWORD_RESET", "User", request.UserId,
                    request, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password reset initiated successfully"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error forcing password reset for user {UserId}", request.UserId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while initiating password reset"
                });
            }
        }

        [HttpGet("statistics")]
        [Authorize(Policy = "CanViewAnalytics")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                var stats = await userServiceClient.GetUserStatisticsAsync();
                return Ok(new ApiResponse<Dictionary<string, object>>
                {
                    Success = true,
                    Data = stats,
                    Message = "User statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting user statistics");
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