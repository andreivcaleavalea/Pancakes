using AdminService.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdminService.Services.Implementations
{
    public class ServiceJwtService : IServiceJwtService
    {
        public string GenerateServiceToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required");
            var key = Encoding.ASCII.GetBytes(secretKey);

            // Create claims for service-to-service communication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "AdminService"),
                new Claim(ClaimTypes.Name, "Admin Service"),
                new Claim(ClaimTypes.Email, "admin-service@pancakes.local"),
                new Claim("service", "true"),
                new Claim("image", ""),
                new Claim("provider", "internal"),
                new Claim("provider_user_id", "AdminService"),
                new Claim("created_at", DateTime.UtcNow.ToString("O")),
                new Claim("last_login_at", DateTime.UtcNow.ToString("O"))
            };

            // Use BlogService JWT settings for compatibility (must match blog's validator)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Short-lived token
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesAdmin",
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesAdminUsers",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
