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
            return await _authService.LoginAsync(HttpContext, request);
        }

        [HttpGet("{provider}/callback")]
        public IActionResult OAuthCallback(string provider, [FromQuery] string code, [FromQuery] string state)
        {
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