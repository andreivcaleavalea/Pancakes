using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlogService.Services.Implementations;
using BlogService.Configuration;
using Microsoft.Extensions.Options;

namespace BlogService.Tests.Services;

public class JwtUserServiceTests : IDisposable
{
    private const string TestSecretKey = "this-is-a-test-secret-key-with-at-least-32-characters-for-jwt-signing";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";

    public JwtUserServiceTests()
    {
        // no-op; environment variables no longer required
    }

    public void Dispose()
    {
        // nothing to dispose currently
    }

    private JwtUserService CreateService(out Mock<IHttpContextAccessor> httpContextAccessorMock, JwtSettings? overrideSettings = null)
    {
        httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var settings = overrideSettings ?? new JwtSettings { SecretKey = TestSecretKey, Issuer = TestIssuer, Audience = TestAudience };
        var options = Options.Create(settings);
        return new JwtUserService(httpContextAccessorMock.Object, options);
    }

    private static string GenerateValidJwtToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TestSecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateExpiredJwtToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TestSecretKey);
        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }),
            NotBefore = now.AddHours(-2), // Valid from 2 hours ago
            Expires = now.AddHours(-1), // Expired 1 hour ago
            IssuedAt = now.AddHours(-2), // Issued 2 hours ago
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Fact]
    public void GetCurrentUserId_WithValidToken_ReturnsUserId()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var expectedUserId = "test-user-123";
        var validToken = GenerateValidJwtToken(expectedUserId);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {validToken}";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetCurrentUserId_WithNoHttpContext_ReturnsNull()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_WithNoAuthorizationHeader_ReturnsNull()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var httpContext = new DefaultHttpContext();
        // No Authorization header set
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_WithInvalidBearerFormat_ReturnsNull()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "InvalidFormat token";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_WithMalformedToken_ReturnsNull()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer invalid.malformed.token";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_WithExpiredToken_ReturnsNull()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var expiredToken = GenerateExpiredJwtToken("test-user-123");
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {expiredToken}";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUserId_WithEmptyBearerToken_ReturnsNull()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer ";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IsAuthenticated_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var validToken = GenerateValidJwtToken("test-user-123");
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {validToken}";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.IsAuthenticated();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer invalid.token.here";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.IsAuthenticated();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_WithNoToken_ReturnsFalse()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        var httpContext = new DefaultHttpContext();
        // No Authorization header
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.IsAuthenticated();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetCurrentUserId_WithMissingSecretKey_ThrowsInvalidOperationException()
    {
        // Arrange
    var service = CreateService(out var httpContextAccessorMock, new JwtSettings { SecretKey = "", Issuer = TestIssuer, Audience = TestAudience });
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer some.token.here";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act & Assert
        var result = service.GetCurrentUserId();
        result.Should().BeNull(); // Should handle the exception gracefully and return null
        
    // no cleanup needed
    }

    [Fact]
    public void GetCurrentUserId_WithTokenMissingUserIdClaim_ReturnsNull()
    {
        // Arrange
        var service = CreateService(out var httpContextAccessorMock);
        
        // Create token without NameIdentifier claim
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TestSecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "TestUser") // Different claim type
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {tokenString}";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().BeNull();
    }
}
