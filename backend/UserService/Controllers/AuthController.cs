using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Services.Interfaces;
using UserService.Models.Requests;
using UserService.Models.Responses;
using Microsoft.Extensions.Logging;

namespace UserService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login request received - Provider: {Provider}, Code: {CodePrefix}..., State: {State}", 
                request.Provider, 
                string.IsNullOrEmpty(request.Code) ? "NULL" : request.Code.Substring(0, Math.Min(10, request.Code.Length)), 
                request.State);

            return await _authService.LoginAsync(HttpContext, request);
        }

        [HttpGet("{provider}/callback")]
        public IActionResult OAuthCallback(string provider, [FromQuery] string code, [FromQuery] string state)
        {
            _logger.LogInformation("OAuth callback received for {Provider}", provider);
            return _authService.OAuthCallback(provider, code, state);
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