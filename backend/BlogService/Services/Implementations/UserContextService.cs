using BlogService.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace BlogService.Services.Implementations;

public class UserContextService : IUserContextService
{
    private readonly IJwtUserService _jwtUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(
        IJwtUserService jwtUserService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserContextService> logger)
    {
        _jwtUserService = jwtUserService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string GetCurrentUserId(HttpContext? httpContext = null)
    {
        // Try to get user ID from JWT token first
        var userId = _jwtUserService.GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }

        // Fallback to anonymous user ID
        var context = httpContext ?? _httpContextAccessor.HttpContext;
        if (context != null)
        {
            return GetAnonymousUserId(context);
        }

        _logger.LogWarning("Unable to determine user ID - no HttpContext available");
        return "anonymous-user";
    }

    public string GetAnonymousUserId(HttpContext httpContext)
    {
        // Generate anonymous user ID based on IP address + UserAgent
        var userIP = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        
        // Create a hash of IP + UserAgent for a semi-persistent anonymous ID
        var combined = $"{userIP}:{userAgent}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        var anonymousId = Convert.ToBase64String(hash)[..12]; // Take first 12 characters
        
        return $"anon-{anonymousId}";
    }
}
