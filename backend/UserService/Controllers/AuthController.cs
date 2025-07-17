using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Services.Interfaces;
using UserService.Models.Requests;
using UserService.Models.Responses;

namespace UserService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine($"Login request received - Provider: {request.Provider}, Code: {(string.IsNullOrEmpty(request.Code) ? "NULL" : request.Code.Substring(0, Math.Min(10, request.Code.Length)))}..., State: {request.State}");

                var userInfo = await _oauthService.ExchangeCodeForUserInfo(request.Code, request.Provider);
                if (userInfo == null)
                {
                    return BadRequest(new { message = "Failed to get user information" });
                }

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
                
                var frontendUrl = $"http://localhost:3000/auth/callback?code={code}&state={state}&provider={provider}";
                return Redirect(frontendUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OAuth callback error: {ex.Message}");
                return Redirect("http://localhost:3000/login?error=callback_failed");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return _authService.Logout(HttpContext);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            return _authService.GetCurrentUser(HttpContext);
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            return _authService.ValidateToken(HttpContext);
        }
    }
}