using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Services.Implementations;
using AdminService.Services.Interfaces;
using AdminService.Tests.TestUtilities;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Services;

public class AdminAuthServiceTests : IDisposable
{
    private readonly AdminDbContext _context;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AdminAuthService _service;

    public AdminAuthServiceTests()
    {
        _context = MockDbContextFactory.CreateInMemoryContext();
        var mockMapper = new Mock<IMapper>();
        _mockConfiguration = new Mock<IConfiguration>();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<AdminAuthService>>();

        SetupConfiguration();
        
        _service = new AdminAuthService(
            _context,
            mockMapper.Object,
            _mockConfiguration.Object,
            mockAuditService.Object,
            mockLogger.Object);
    }

    private void SetupConfiguration()
    {
        // Set environment variable for JWT
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "ThisIsAVeryLongSecretKeyForJWTTokenGeneration123456789");
        
        _mockConfiguration.Setup(x => x["Jwt:Key"])
            .Returns("ThisIsAVeryLongSecretKeyForJWTTokenGeneration123456789");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"])
            .Returns("AdminService");
        _mockConfiguration.Setup(x => x["Jwt:Audience"])
            .Returns("AdminPanel");
        _mockConfiguration.Setup(x => x["Jwt:ExpiryInHours"])
            .Returns("24");
    }

    [Fact]
    public async Task HasAdminUsersAsync_WhenAdminsExist_ReturnsTrue()
    {
        var adminUser = TestDataHelper.CreateTestAdminUser();
        await _context.AdminUsers.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        var result = await _service.HasAdminUsersAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAdminUsersAsync_WhenNoAdminsExist_ReturnsFalse()
    {

        var result = await _service.HasAdminUsersAsync();

        result.Should().BeFalse();
    }





    [Fact]
    public async Task ValidateTokenAsync_WhenInvalidToken_ReturnsFalse()
    {
        var invalidToken = "invalid.jwt.token";

        var result = await _service.ValidateTokenAsync(invalidToken);

        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_WhenCalled_ReturnsHashedPassword()
    {
        var password = "mySecretPassword123";

        var hashedPassword = _service.HashPassword(password);

        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        
        _service.VerifyPassword(password, hashedPassword).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WhenPasswordMatches_ReturnsTrue()
    {
        var password = "mySecretPassword123";
        var hashedPassword = _service.HashPassword(password); 

        var result = _service.VerifyPassword(password, hashedPassword);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WhenPasswordDoesNotMatch_ReturnsFalse()
    {
        var password = "mySecretPassword123";
        var wrongPassword = "wrongPassword456";
        var hashedPassword = _service.HashPassword(password); 

        var result = _service.VerifyPassword(wrongPassword, hashedPassword);

        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateJwtToken_WhenCalled_ReturnsValidToken()
    {
        var adminDto = new AdminUserDto
        {
            Id = "admin-id",
            Email = "test@admin.com",
            Name = "Test Admin",
            AdminLevel = 1
        };

        var token = _service.GenerateJwtToken(adminDto);

        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts separated by dots
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}