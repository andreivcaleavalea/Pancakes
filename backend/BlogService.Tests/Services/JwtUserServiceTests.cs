using BlogService.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogService.Tests.Services;

public class JwtUserServiceTests
{
    private const string TestSecretKey = "this-is-a-test-secret-key-that-is-long-enough-for-256-bit-encryption";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";

    private static JwtUserService CreateService(HttpContext? httpContext = null)
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);
        return new JwtUserService(httpContextAccessor.Object);
    }

    private static HttpContext CreateHttpContextWithToken(string? token = null)
    {
        var context = new DefaultHttpContext();
        if (token != null)
        {
            context.Request.Headers.Authorization = $"Bearer {token}";
        }
        return context;
    }

    private static string CreateValidJwtToken(string userId = "test-user-123")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateExpiredJwtToken(string userId = "test-user-123")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test User")
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(-1), // Expired
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public JwtUserServiceTests()
    {
        // Set environment variables for tests
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", TestSecretKey);
        Environment.SetEnvironmentVariable("JWT_ISSUER", TestIssuer);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", TestAudience);
    }

    [Fact]
    public void GetCurrentUserId_NoHttpContext_ReturnsNull()
    {
        var service = CreateService(null);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_NoAuthorizationHeader_ReturnsNull()
    {
        var httpContext = CreateHttpContextWithToken(null);
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_InvalidAuthorizationHeader_ReturnsNull()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "InvalidHeader";
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_EmptyBearerToken_ReturnsNull()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer ";
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_ValidToken_ReturnsUserId()
    {
        var expectedUserId = "user-123";
        var token = CreateValidJwtToken(expectedUserId);
        var httpContext = CreateHttpContextWithToken(token);
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetCurrentUserId_ExpiredToken_ReturnsNull()
    {
        var token = CreateExpiredJwtToken("user-123");
        var httpContext = CreateHttpContextWithToken(token);
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_InvalidToken_ReturnsNull()
    {
        var httpContext = CreateHttpContextWithToken("invalid.jwt.token");
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_TokenWithDifferentIssuer_ReturnsNull()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
        };

        var token = new JwtSecurityToken(
            issuer: "WrongIssuer", // Different issuer
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var httpContext = CreateHttpContextWithToken(tokenString);
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_TokenWithDifferentAudience_ReturnsNull()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: "WrongAudience", // Different audience
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var httpContext = CreateHttpContextWithToken(tokenString);
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_TokenWithoutNameIdentifierClaim_ReturnsNull()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"), // No NameIdentifier claim
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var httpContext = CreateHttpContextWithToken(tokenString);
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void IsAuthenticated_ValidToken_ReturnsTrue()
    {
        var token = CreateValidJwtToken("user-123");
        var httpContext = CreateHttpContextWithToken(token);
        var service = CreateService(httpContext);

        var result = service.IsAuthenticated();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_NoToken_ReturnsFalse()
    {
        var httpContext = CreateHttpContextWithToken(null);
        var service = CreateService(httpContext);

        var result = service.IsAuthenticated();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_InvalidToken_ReturnsFalse()
    {
        var httpContext = CreateHttpContextWithToken("invalid.token");
        var service = CreateService(httpContext);

        var result = service.IsAuthenticated();

        result.Should().BeFalse();
    }

    [Fact]
    public void GetCurrentUserId_MissingSecretKey_ReturnsNull()
    {
        // Temporarily remove the environment variable
        var originalSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);

        try
        {
            var token = CreateValidJwtToken("user-123");
            var httpContext = CreateHttpContextWithToken(token);
            var service = CreateService(httpContext);

            var result = service.GetCurrentUserId();

            result.Should().BeNull();
        }
        finally
        {
            // Restore the environment variable
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", originalSecret);
        }
    }

    [Fact]
    public void GetCurrentUserId_DefaultIssuerAndAudience_WorksCorrectly()
    {
        // Temporarily remove the environment variables to test defaults
        var originalIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var originalAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        Environment.SetEnvironmentVariable("JWT_ISSUER", null);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);

        try
        {
            // Create token with default values
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
            };

            var token = new JwtSecurityToken(
                issuer: "PancakesBlog", // Default issuer
                audience: "PancakesBlogUsers", // Default audience
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var httpContext = CreateHttpContextWithToken(tokenString);
            var service = CreateService(httpContext);

            var result = service.GetCurrentUserId();

            result.Should().Be("user-123");
        }
        finally
        {
            // Restore the environment variables
            Environment.SetEnvironmentVariable("JWT_ISSUER", originalIssuer);
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", originalAudience);
        }
    }

    [Fact]
    public void GetCurrentUserId_MultipleAuthorizationHeaders_UsesFirst()
    {
        var token = CreateValidJwtToken("user-123");
        var httpContext = new DefaultHttpContext();
        
        // Add multiple authorization headers
        httpContext.Request.Headers.Authorization = new StringValues(new[] 
        { 
            $"Bearer {token}",
            "Bearer invalid-token"
        });
        
        var service = CreateService(httpContext);

        var result = service.GetCurrentUserId();

        result.Should().Be("user-123");
    }
}
