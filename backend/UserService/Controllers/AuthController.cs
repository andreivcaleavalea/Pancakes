using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Services;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly OAuthService _oauthService;
        private readonly UserManagementService _userService;
        private readonly JwtService _jwtService;
        private readonly CurrentUserService _currentUserService;

        public AuthController(
            OAuthService oauthService, 
            UserManagementService userService, 
            JwtService jwtService,
            CurrentUserService currentUserService)
        {
            _oauthService = oauthService;
            _userService = userService;
            _jwtService = jwtService;
            _currentUserService = currentUserService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine($"Login attempt for provider: {request.Provider}");

                var userInfo = await _oauthService.ExchangeCodeForUserInfo(request.Code, request.Provider);
                if (userInfo == null)
                {
                    return BadRequest(new { message = "Failed to get user information" });
                }

                // Create user object (stateless - no persistence)
                var user = _userService.CreateUserFromOAuth(userInfo, request.Provider);
                var token = _jwtService.GenerateToken(user);

                var response = new LoginResponse
                {
                    Token = token,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(1440) // 24 hours
                };

                Console.WriteLine($"Login successful for user: {user.Name}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{provider}/callback")]
        public IActionResult OAuthCallback(string provider, [FromQuery] string code, [FromQuery] string state)
        {
            try
            {
                Console.WriteLine($"OAuth callback received for {provider}");
                
                var frontendUrl = $"http://localhost:5173/auth/callback?code={code}&state={state}&provider={provider}";
                return Redirect(frontendUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OAuth callback error: {ex.Message}");
                return Redirect("http://localhost:5173/login?error=callback_failed");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                var currentUser = _currentUserService.GetCurrentUser();
                if (currentUser != null)
                {
                    Console.WriteLine($"User {currentUser.Name} logged out");
                }
                
                // In a stateless system, logout is handled on the frontend by removing the token
                // No server-side state to clear
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var currentUser = _currentUserService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                Console.WriteLine($"Retrieved current user: {currentUser.Name}");
                return Ok(currentUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get current user error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            try
            {
                var isAuthenticated = _currentUserService.IsAuthenticated();
                var userId = _currentUserService.GetCurrentUserId();
                
                return Ok(new { 
                    isValid = isAuthenticated,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}