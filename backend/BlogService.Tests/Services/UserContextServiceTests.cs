using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;

namespace BlogService.Tests.Services;

public class UserContextServiceTests
{
    private static UserContextService CreateService(
        out Mock<IJwtUserService> jwtUserServiceMock,
        out Mock<IHttpContextAccessor> httpContextAccessorMock,
        out Mock<ILogger<UserContextService>> loggerMock)
    {
        jwtUserServiceMock = new Mock<IJwtUserService>();
        httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        loggerMock = new Mock<ILogger<UserContextService>>();
        
        return new UserContextService(
            jwtUserServiceMock.Object,
            httpContextAccessorMock.Object,
            loggerMock.Object);
    }

    private static DefaultHttpContext CreateHttpContextWithIpAndUserAgent(string ip, string userAgent)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        httpContext.Request.Headers.UserAgent = userAgent;
        return httpContext;
    }

    [Fact]
    public void GetCurrentUserId_WithAuthenticatedUser_ReturnsJwtUserId()
    {
        // Arrange
        var service = CreateService(out var jwtUserServiceMock, out var httpContextAccessorMock, out var loggerMock);
        var expectedUserId = "authenticated-user-123";
        jwtUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(expectedUserId);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().Be(expectedUserId);
        jwtUserServiceMock.Verify(x => x.GetCurrentUserId(), Times.Once);
        httpContextAccessorMock.VerifyNoOtherCalls(); // Should not access context for authenticated users
    }

    [Fact]
    public void GetCurrentUserId_WithUnauthenticatedUserAndContext_ReturnsAnonymousId()
    {
        // Arrange
        var service = CreateService(out var jwtUserServiceMock, out var httpContextAccessorMock, out var loggerMock);
        var httpContext = CreateHttpContextWithIpAndUserAgent("192.168.1.1", "Mozilla/5.0 Test Browser");
        
        jwtUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().StartWith("anon-");
        result.Should().HaveLength(17); // "anon-" + 12 characters from hash
        jwtUserServiceMock.Verify(x => x.GetCurrentUserId(), Times.Once);
        httpContextAccessorMock.Verify(x => x.HttpContext, Times.Once);
    }

    [Fact]
    public void GetCurrentUserId_WithUnauthenticatedUserAndNoContext_ReturnsDefaultAnonymous()
    {
        // Arrange
        var service = CreateService(out var jwtUserServiceMock, out var httpContextAccessorMock, out var loggerMock);
        
        jwtUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().Be("anonymous-user");
        
        // Verify warning was logged
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to determine user ID - no HttpContext available")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetCurrentUserId_WithProvidedHttpContext_UsesProvidedContext()
    {
        // Arrange
        var service = CreateService(out var jwtUserServiceMock, out var httpContextAccessorMock, out var loggerMock);
        var providedContext = CreateHttpContextWithIpAndUserAgent("10.0.0.1", "Custom User Agent");
        
        jwtUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);

        // Act
        var result = service.GetCurrentUserId(providedContext);

        // Assert
        result.Should().StartWith("anon-");
        jwtUserServiceMock.Verify(x => x.GetCurrentUserId(), Times.Once);
        httpContextAccessorMock.VerifyNoOtherCalls(); // Should not access the injected context accessor
    }

    [Fact]
    public void GetAnonymousUserId_WithSameIpAndUserAgent_ReturnsSameId()
    {
        // Arrange
        var service = CreateService(out _, out _, out _);
        var httpContext1 = CreateHttpContextWithIpAndUserAgent("192.168.1.100", "Mozilla/5.0 Chrome");
        var httpContext2 = CreateHttpContextWithIpAndUserAgent("192.168.1.100", "Mozilla/5.0 Chrome");

        // Act
        var id1 = service.GetAnonymousUserId(httpContext1);
        var id2 = service.GetAnonymousUserId(httpContext2);

        // Assert
        id1.Should().Be(id2);
        id1.Should().StartWith("anon-");
        id1.Should().HaveLength(17); // "anon-" + 12 characters
    }

    [Fact]
    public void GetAnonymousUserId_WithDifferentIp_ReturnsDifferentId()
    {
        // Arrange
        var service = CreateService(out _, out _, out _);
        var httpContext1 = CreateHttpContextWithIpAndUserAgent("192.168.1.1", "Mozilla/5.0 Chrome");
        var httpContext2 = CreateHttpContextWithIpAndUserAgent("192.168.1.2", "Mozilla/5.0 Chrome");

        // Act
        var id1 = service.GetAnonymousUserId(httpContext1);
        var id2 = service.GetAnonymousUserId(httpContext2);

        // Assert
        id1.Should().NotBe(id2);
        id1.Should().StartWith("anon-");
        id2.Should().StartWith("anon-");
    }

    [Fact]
    public void GetAnonymousUserId_WithDifferentUserAgent_ReturnsDifferentId()
    {
        // Arrange
        var service = CreateService(out _, out _, out _);
        var httpContext1 = CreateHttpContextWithIpAndUserAgent("192.168.1.1", "Mozilla/5.0 Chrome");
        var httpContext2 = CreateHttpContextWithIpAndUserAgent("192.168.1.1", "Mozilla/5.0 Firefox");

        // Act
        var id1 = service.GetAnonymousUserId(httpContext1);
        var id2 = service.GetAnonymousUserId(httpContext2);

        // Assert
        id1.Should().NotBe(id2);
        id1.Should().StartWith("anon-");
        id2.Should().StartWith("anon-");
    }

    [Fact]
    public void GetAnonymousUserId_WithNullIpAddress_HandlesGracefully()
    {
        // Arrange
        var service = CreateService(out _, out _, out _);
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = null; // Null IP
        httpContext.Request.Headers.UserAgent = "Test Browser";

        // Act
        var result = service.GetAnonymousUserId(httpContext);

        // Assert
        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
    }

    [Fact]
    public void GetAnonymousUserId_WithEmptyUserAgent_HandlesGracefully()
    {
        // Arrange
        var service = CreateService(out _, out _, out _);
        var httpContext = CreateHttpContextWithIpAndUserAgent("192.168.1.1", "");

        // Act
        var result = service.GetAnonymousUserId(httpContext);

        // Assert
        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
    }

    [Fact]
    public void GetAnonymousUserId_WithLongUserAgent_HandlesCorrectly()
    {
        // Arrange
        var service = CreateService(out _, out _, out _);
        var longUserAgent = new string('A', 1000); // Very long user agent
        var httpContext = CreateHttpContextWithIpAndUserAgent("192.168.1.1", longUserAgent);

        // Act
        var result = service.GetAnonymousUserId(httpContext);

        // Assert
        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
    }

    [Fact]
    public void GetAnonymousUserId_WithSpecialCharactersInUserAgent_HandlesCorrectly()
    {
        // Arrange
        var service = CreateService(out _, out _, out _);
        var specialUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 !@#$%^&*()";
        var httpContext = CreateHttpContextWithIpAndUserAgent("192.168.1.1", specialUserAgent);

        // Act
        var result = service.GetAnonymousUserId(httpContext);

        // Assert
        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
    }

    [Fact]
    public void GetAnonymousUserId_GeneratesConsistentHashFormat()
    {
        // Arrange
        var service = CreateService(out _, out _, out _);
        var httpContext = CreateHttpContextWithIpAndUserAgent("127.0.0.1", "Test Browser");

        // Act
        var result = service.GetAnonymousUserId(httpContext);

        // Assert
        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
        result.Substring(5).Should().MatchRegex("^[A-Za-z0-9+/]{12}$"); // Base64 characters
    }

    [Fact]
    public void GetCurrentUserId_EmptyJwtUserId_FallsBackToAnonymous()
    {
        // Arrange
        var service = CreateService(out var jwtUserServiceMock, out var httpContextAccessorMock, out var loggerMock);
        var httpContext = CreateHttpContextWithIpAndUserAgent("192.168.1.1", "Mozilla/5.0 Test Browser");
        
        jwtUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(string.Empty); // Empty string
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = service.GetCurrentUserId();

        // Assert
        result.Should().StartWith("anon-");
    }
}
