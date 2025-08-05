using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AdminService.Validations;
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

        [HttpPost("bootstrap")]
        public async Task<IActionResult> Bootstrap([FromBody] CreateAdminUserRequest request)
        {
            try
            {
                var validationResult = CreateAdminRequestValidator.ValidateCreateAdminRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = validationResult.Errors.ToList()
                    });
                }

                // Check if any admin users already exist
                var hasAdmins = await _adminAuthService.HasAdminUsersAsync();
                if (hasAdmins)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Admin users already exist. Bootstrap is only available for initial setup."
                    });
                }

                var admin = await _adminAuthService.CreateBootstrapAdminAsync(request);
                
                await _auditService.LogActionAsync("SYSTEM", "BOOTSTRAP_ADMIN_CREATED", "AdminUser", admin.Id,
                    new { request.Email, request.Name, request.AdminLevel }, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = admin,
                    Message = "Bootstrap admin user created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bootstrap admin user");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating bootstrap admin user"
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            try
            {
                var validationResult = LoginRequestValidator.ValidateLoginRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid login data",
                        Errors = validationResult.Errors.ToList()
                    });
                }

                var response = await _adminAuthService.LoginAsync(HttpContext, request);
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

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                // Clear the httpOnly cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(-1), // Set expiry to past date to delete
                    Path = "/"
                };
                HttpContext.Response.Cookies.Append("adminToken", "", cookieOptions);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Logout successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
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