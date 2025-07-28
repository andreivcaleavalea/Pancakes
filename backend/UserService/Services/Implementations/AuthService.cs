using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Models;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IOAuthService _oauthService;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _persistentUserService;
    private readonly IUserMappingService _userMappingService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IOAuthService oauthService,
        IJwtService jwtService,
        ICurrentUserService currentUserService,
        IUserService persistentUserService,
        IUserMappingService userMappingService,
        ILogger<AuthService> logger)
    {
        _oauthService = oauthService;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
        _persistentUserService = persistentUserService;
        _userMappingService = userMappingService;
        _logger = logger;
    }

    public async Task<IActionResult> LoginAsync(HttpContext httpContext, LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login request received - Provider: {Provider}, Code: {Code}, State: {State}", 
                request.Provider, 
                string.IsNullOrEmpty(request.Code) ? "NULL" : request.Code.Substring(0, Math.Min(10, request.Code.Length)) + "...", 
                request.State);

            var userInfo = await _oauthService.ExchangeCodeForUserInfo(request.Code, request.Provider);
            if (userInfo == null)
            {
                return new BadRequestObjectResult(new { message = "Failed to get user information" });
            }

            // Create or update user in database (persistent)
            var userDto = await _persistentUserService.CreateOrUpdateFromOAuthAsync(userInfo, request.Provider);
            
            // Convert DTO back to User entity for JWT token generation using mapping service
            var user = _userMappingService.MapUserDtoToUser(userDto);

            var token = _jwtService.GenerateToken(user);

            var response = new LoginResponse
            {
                Token = token,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddMinutes(1440) // 24 hours
            };

            _logger.LogInformation("Login successful for user: {UserName}", user.Name);
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public IActionResult OAuthCallback(string provider, string code, string state)
    {
        try
        {
            _logger.LogInformation("OAuth callback received for {Provider}", provider);
            
            var frontendUrl = $"http://localhost:5173/auth/callback?code={code}&state={state}&provider={provider}";
            return new RedirectResult(frontendUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth callback error: {Message}", ex.Message);
            return new RedirectResult("http://localhost:5173/login?error=callback_failed");
        }
    }

    public IActionResult Logout(HttpContext httpContext)
    {
        try
        {
            var currentUser = _currentUserService.GetCurrentUser();
            if (currentUser != null)
            {
                _logger.LogInformation("User {UserName} logged out", currentUser.Name);
            }
            
            // In a stateless system, logout is handled on the frontend by removing the token
            // No server-side state to clear
            return new OkObjectResult(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public IActionResult GetCurrentUser(HttpContext httpContext)
    {
        try
        {
            var currentUser = _currentUserService.GetCurrentUser();
            if (currentUser == null)
            {
                return new UnauthorizedObjectResult(new { message = "User not authenticated" });
            }

            _logger.LogInformation("Retrieved current user: {UserName}", currentUser.Name);
            return new OkObjectResult(currentUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current user error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public IActionResult ValidateToken(HttpContext httpContext)
    {
        try
        {
            var isAuthenticated = _currentUserService.IsAuthenticated();
            var userId = _currentUserService.GetCurrentUserId();
            
            return new OkObjectResult(new { 
                isValid = isAuthenticated,
                userId = userId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }
}
