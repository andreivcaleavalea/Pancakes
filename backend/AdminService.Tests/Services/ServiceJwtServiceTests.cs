using AdminService.Services.Implementations;
using AdminService.Services.Interfaces;
using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace AdminService.Tests.Services;

public class ServiceJwtServiceTests : IDisposable
{
    private readonly IServiceJwtService _service;
    private readonly string? _originalJwtSecret;
    private readonly string? _originalJwtIssuer;
    private readonly string? _originalJwtAudience;

    public ServiceJwtServiceTests()
    {
        // Store original environment values
        _originalJwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        _originalJwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        _originalJwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        // Set test environment values
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test-super-secret-jwt-key-at-least-32-characters-long-for-testing");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestPancakesAdmin");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestPancakesAdminUsers");

        _service = new ServiceJwtService();
    }

    [Fact]
    public void GenerateServiceToken_WhenCalled_ReturnsValidJwtToken()
    {
        var token = _service.GenerateServiceToken();

        token.Should().NotBeNullOrWhiteSpace();
        
        var parts = token.Split('.');
        parts.Should().HaveCount(3, "JWT should have header, payload, and signature");
    }

    [Fact]
    public void GenerateServiceToken_WhenCalled_TokenShouldBeDecodable()
    {
        var token = _service.GenerateServiceToken();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Should().NotBeNull();
        jsonToken.Header.Should().NotBeNull();
        jsonToken.Payload.Should().NotBeNull();
    }

    [Fact]
    public void GenerateServiceToken_WhenCalled_ContainsRequiredClaims()
    {
        // Act
        var token = _service.GenerateServiceToken();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var claims = jsonToken.Claims.ToList();

        // Verify required claims exist - use actual claim types from JWT
        claims.Should().Contain(c => c.Type == "nameid" && c.Value == "AdminService");
        claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "Admin Service");
        claims.Should().Contain(c => c.Type == "email" && c.Value == "admin-service@pancakes.local");
        claims.Should().Contain(c => c.Type == "service" && c.Value == "true");
        claims.Should().Contain(c => c.Type == "provider" && c.Value == "internal");
        claims.Should().Contain(c => c.Type == "provider_user_id" && c.Value == "AdminService");
    }

    [Fact]
    public void GenerateServiceToken_WhenCalled_ContainsTimestampClaims()
    {
        // Arrange
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _service.GenerateServiceToken();

        // Assert
        var afterGeneration = DateTime.UtcNow;
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var createdAtClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "created_at");
        var lastLoginAtClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "last_login_at");

        createdAtClaim.Should().NotBeNull();
        lastLoginAtClaim.Should().NotBeNull();

        // Verify timestamps are valid and within expected range
        var createdAt = DateTime.Parse(createdAtClaim!.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var lastLoginAt = DateTime.Parse(lastLoginAtClaim!.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);

        // Allow for a wider time range to account for execution time and potential timezone differences
        createdAt.Should().BeOnOrAfter(beforeGeneration.AddSeconds(-5));
        createdAt.Should().BeOnOrBefore(afterGeneration.AddSeconds(5));
        lastLoginAt.Should().BeOnOrAfter(beforeGeneration.AddSeconds(-5));
        lastLoginAt.Should().BeOnOrBefore(afterGeneration.AddSeconds(5));
    }

    [Fact]
    public void GenerateServiceToken_WhenCalled_ContainsEmptyImageClaim()
    {
        var token = _service.GenerateServiceToken();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var imageClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "image");
        imageClaim.Should().NotBeNull();
        imageClaim!.Value.Should().Be("");
    }

    [Fact]
    public void GenerateServiceToken_WhenCalled_HasCorrectIssuerAndAudience()
    {
        var token = _service.GenerateServiceToken();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Issuer.Should().Be("TestPancakesAdmin");
        jsonToken.Audiences.Should().Contain("TestPancakesAdminUsers");
    }

    [Fact]
    public void GenerateServiceToken_WhenJwtSecretMissing_ThrowsInvalidOperationException()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);

        var action = () => _service.GenerateServiceToken();
        
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT_SECRET_KEY*");
    }

    [Fact]
    public void GenerateServiceToken_WhenJwtSecretEmpty_ThrowsException()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "");

        var action = () => _service.GenerateServiceToken();
        
        action.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData(null, "PancakesBlog")]
    public void GenerateServiceToken_WhenIssuerMissing_UsesDefaultIssuer(string? issuer, string expectedIssuer)
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_ISSUER", issuer);

        // Act
        var token = _service.GenerateServiceToken();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        jsonToken.Issuer.Should().Be(expectedIssuer);
    }

    [Fact]
    public void GenerateServiceToken_WhenIssuerEmpty_UsesEmptyIssuer()
    {
        // Arrange - empty string is different from null, so it won't use default
        Environment.SetEnvironmentVariable("JWT_ISSUER", "");

        // Act
        var token = _service.GenerateServiceToken();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        // When issuer is empty string, JWT library treats it as null
        jsonToken.Issuer.Should().BeNull();
    }
    
    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", _originalJwtSecret);
        Environment.SetEnvironmentVariable("JWT_ISSUER", _originalJwtIssuer);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", _originalJwtAudience);
    }
}
