using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations
{
    /// <summary>
    /// Service for extracting user information from JWT tokens in BlogService
    /// </summary>
    public class JwtUserService : IJwtUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the current user's ID from the JWT token in the authorization header.
        /// </summary>
        /// <returns>User ID if authenticated, null otherwise</returns>
        public string? GetCurrentUserId()
        {
            var token = ExtractTokenFromAuthorizationHeader();
            if (string.IsNullOrEmpty(token))
                return null;

            return ExtractUserIdFromToken(token);
        }

        /// <summary>
        /// Checks if the current request has a valid JWT token.
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetCurrentUserId());
        }

        /// <summary>
        /// Extracts the JWT token from the Authorization header.
        /// </summary>
        /// <returns>JWT token string if present, null otherwise</returns>
        private string? ExtractTokenFromAuthorizationHeader()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return null;

            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            return authHeader.Substring("Bearer ".Length);
        }

        /// <summary>
        /// Extracts the user ID from a JWT token.
        /// </summary>
        /// <param name="token">JWT token string</param>
        /// <returns>User ID if token is valid, null otherwise</returns>
        private string? ExtractUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                    ?? throw new InvalidOperationException("JWT_SECRET_KEY must be set");
                var key = Encoding.UTF8.GetBytes(secretKey);
                
                var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesBlog";
                var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesBlogUsers";

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting user ID from token: {ex.Message}");
                return null;
            }
        }
    }
}
