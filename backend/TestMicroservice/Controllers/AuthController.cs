using Microsoft.AspNetCore.Mvc;
using TestMicroservice.Models;
using TestMicroservice.Services;

namespace TestMicroservice.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly OAuthService _oauthService;
        private readonly UserService _userService;
        private readonly JwtService _jwtService;

        public AuthController(
            OAuthService oauthService, 
            UserService userService, 
            JwtService jwtService)
        {
            _oauthService = oauthService;
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.Provider))
                {
                    return BadRequest(new { message = "Code and provider are required" });
                }

                // Only support Google for now
                if (request.Provider.ToLower() != "google")
                {
                    return BadRequest(new { message = "Only Google authentication is currently supported" });
                }

                // Exchange code for user info
                var userInfo = await _oauthService.ExchangeCodeForUserInfo(request.Code, request.Provider);
                if (userInfo == null)
                {
                    return BadRequest(new { message = "Failed to exchange OAuth code" });
                }

                // Create or update user
                var user = _userService.CreateOrUpdateUser(userInfo, request.Provider);

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

                var response = new LoginResponse
                {
                    Token = token,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(1440) // 24 hours
                };

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
            // Only support Google for now
            if (provider.ToLower() != "google")
            {
                var errorUrl = $"http://localhost:5173/login?error=unsupported_provider";
                return Redirect(errorUrl);
            }

            // This endpoint receives the OAuth callback and redirects back to frontend
            var frontendUrl = $"http://localhost:5173/auth/callback?code={code}&state={state}&provider={provider}";
            return Redirect(frontendUrl);
        }
    }
}