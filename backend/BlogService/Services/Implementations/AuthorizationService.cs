using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IUserServiceClient userServiceClient,
        ILogger<AuthorizationService> logger)
    {
        _userServiceClient = userServiceClient;
        _logger = logger;
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync(HttpContext httpContext)
    {
        try
        {
            var token = ExtractTokenFromHeader(httpContext);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No authorization token found");
                return null;
            }

            var currentUser = await _userServiceClient.GetCurrentUserAsync(token);
            if (currentUser == null)
            {
                _logger.LogWarning("Failed to get current user from UserService");
                return null;
            }

            _logger.LogDebug("Current user retrieved: Id={UserId}, Name={UserName}", 
                currentUser.Id, currentUser.Name);

            return currentUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return null;
        }
    }

    public string ExtractTokenFromHeader(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return string.Empty;
        }

        return authHeader.Substring("Bearer ".Length);
    }

    public bool IsTokenValid(string token)
    {
        return !string.IsNullOrEmpty(token) && token.Length > 10; // Basic validation
    }
}
