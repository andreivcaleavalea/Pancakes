using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;

namespace UserService.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public string GenerateToken(User user)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? throw new InvalidOperationException("JWT_SECRET_KEY must be set");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("image", user.Image),
                new Claim("provider", user.Provider),
                new Claim("provider_user_id", user.ProviderUserId),
                new Claim("created_at", user.CreatedAt.ToString("O")), // ISO 8601 format
                new Claim("last_login_at", user.LastLoginAt.ToString("O")) // ISO 8601 format
            };

            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesBlog";
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesBlogUsers";
            var expirationMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "1440");

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        public User? GetUserFromToken(string token)
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
                return ExtractUserFromClaims(principal.Claims);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
                return null;
            }
        }
        
        private User ExtractUserFromClaims(IEnumerable<Claim> claims)
        {
            var claimsDictionary = claims.ToDictionary(x => x.Type, x => x.Value);

            return new User
            {
                Id = claimsDictionary[ClaimTypes.NameIdentifier],
                Name = claimsDictionary[ClaimTypes.Name],
                Email = claimsDictionary[ClaimTypes.Email],
                Image = claimsDictionary.GetValueOrDefault("image", ""),
                Provider = claimsDictionary.GetValueOrDefault("provider", ""),
                ProviderUserId = claimsDictionary.GetValueOrDefault("provider_user_id", ""),
                CreatedAt = DateTime.Parse(claimsDictionary.GetValueOrDefault("created_at", DateTime.UtcNow.ToString("O"))),
                LastLoginAt = DateTime.Parse(claimsDictionary.GetValueOrDefault("last_login_at", DateTime.UtcNow.ToString("O")))
            };
        }

        public bool ValidateToken(string token)
        {
            return GetUserFromToken(token) != null;
        }
    }
}