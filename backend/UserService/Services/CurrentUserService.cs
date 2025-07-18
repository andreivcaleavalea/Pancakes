using System.Security.Claims;
using UserService.Models;

namespace UserService.Services
{
    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

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

        public string? GetCurrentUserId()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }


        public bool IsAuthenticated()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
