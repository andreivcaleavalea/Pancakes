using System.Security.Claims;
using TestMicroservice.Models;

namespace TestMicroservice.Services
{
    /// <summary>
    /// Service for accessing the current authenticated user from the HTTP context.
    /// This provides a clean abstraction for getting user information from JWT tokens.
    /// </summary>
    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the current authenticated user from the JWT token in the request.
        /// </summary>
        /// <returns>Current user if authenticated, null otherwise</returns>
        public User? GetCurrentUser()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated != true)
                return null;

            try
            {
                var claims = context.User.Claims.ToDictionary(x => x.Type, x => x.Value);

                return new User
                {
                    Id = claims.GetValueOrDefault(ClaimTypes.NameIdentifier, ""),
                    Name = claims.GetValueOrDefault(ClaimTypes.Name, ""),
                    Email = claims.GetValueOrDefault(ClaimTypes.Email, ""),
                    Image = claims.GetValueOrDefault("image", ""),
                    Provider = claims.GetValueOrDefault("provider", ""),
                    ProviderUserId = claims.GetValueOrDefault("provider_user_id", ""),
                    CreatedAt = DateTime.TryParse(claims.GetValueOrDefault("created_at", ""), out var createdAt) 
                        ? createdAt 
                        : DateTime.UtcNow,
                    LastLoginAt = DateTime.TryParse(claims.GetValueOrDefault("last_login_at", ""), out var lastLoginAt) 
                        ? lastLoginAt 
                        : DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting user from claims: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the current user's ID.
        /// </summary>
        /// <returns>User ID if authenticated, null otherwise</returns>
        public string? GetCurrentUserId()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Checks if the current request is authenticated.
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        public bool IsAuthenticated()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
