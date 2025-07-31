using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthService _adminAuthService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AdminAuthController> _logger;

        public AdminAuthController(
            IAdminAuthService adminAuthService,
            IAuditService auditService,
            ILogger<AdminAuthController> logger)
        {
            _adminAuthService = adminAuthService;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            try
            {
                var response = await _adminAuthService.LoginAsync(request);
                return Ok(new ApiResponse<AdminLoginResponse>
                {
                    Success = true,
                    Data = response,
                    Message = "Login successful"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, ex.Message);
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentAdmin()
        {
            try
            {
                var token = GetTokenFromHeader();
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No token provided"
                    });
                }

                var admin = await _adminAuthService.GetCurrentAdminAsync(token);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = admin,
                    Message = "Admin retrieved successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current admin");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        [HttpPost("admin-users")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdminUser([FromBody] CreateAdminUserRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var admin = await _adminAuthService.CreateAdminUserAsync(request, currentAdminId);
                
                await _auditService.LogActionAsync(currentAdminId, "ADMIN_USER_CREATED", "AdminUser", admin.Id,
                    new { request.Email, request.Name, request.AdminLevel }, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = admin,
                    Message = "Admin user created successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating admin user"
                });
            }
        }

        [HttpPut("admin-users/{adminId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateAdminUser(string adminId, [FromBody] UpdateAdminUserRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var admin = await _adminAuthService.UpdateAdminUserAsync(adminId, request, currentAdminId);
                
                await _auditService.LogActionAsync(currentAdminId, "ADMIN_USER_UPDATED", "AdminUser", adminId,
                    request, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = admin,
                    Message = "Admin user updated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating admin user {AdminId}", adminId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating admin user"
                });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                await _adminAuthService.ChangePasswordAsync(currentAdminId, request);
                
                await _auditService.LogActionAsync(currentAdminId, "PASSWORD_CHANGED", "AdminUser", currentAdminId,
                    null, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password changed successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                });
            }
        }

        [HttpGet("admin-users")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAdminUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var adminUsers = await _adminAuthService.GetAdminUsersAsync(page, pageSize);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = adminUsers,
                    Message = "Admin users retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin users");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving admin users"
                });
            }
        }

        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var token = GetTokenFromHeader();
                var isValid = await _adminAuthService.ValidateTokenAsync(token);
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { IsValid = isValid },
                    Message = "Token validation completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during token validation"
                });
            }
        }

        private string GetTokenFromHeader()
        {
            var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length);
            }
            return string.Empty;
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