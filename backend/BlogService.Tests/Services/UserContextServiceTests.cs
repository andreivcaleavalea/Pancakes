using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BlogService.Tests.Services;

public class UserContextServiceTests
{
    private static UserContextService CreateService(
        out Mock<IJwtUserService> jwtUserService,
        HttpContext? httpContext = null)
    {
        jwtUserService = new Mock<IJwtUserService>(MockBehavior.Strict);
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);
        var logger = new Mock<ILogger<UserContextService>>();

        return new UserContextService(jwtUserService.Object, httpContextAccessor.Object, logger.Object);
    }

    private static HttpContext CreateHttpContext(string? ipAddress = null, string? userAgent = null)
    {
        var context = new DefaultHttpContext();
        
        if (ipAddress != null)
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        }
        
        if (userAgent != null)
        {
            context.Request.Headers.UserAgent = userAgent;
        }

        return context;
    }

    [Fact]
    public void GetCurrentUserId_AuthenticatedUser_ReturnsJwtUserId()
    {
        var service = CreateService(out var jwtUserService);
        jwtUserService.Setup(j => j.GetCurrentUserId()).Returns("jwt-user-123");

        var result = service.GetCurrentUserId();

        result.Should().Be("jwt-user-123");
    }

    [Fact]
    public void GetCurrentUserId_NotAuthenticated_WithHttpContext_ReturnsAnonymousId()
    {
        var httpContext = CreateHttpContext("192.168.1.1", "Mozilla/5.0");
        var service = CreateService(out var jwtUserService, httpContext);
        jwtUserService.Setup(j => j.GetCurrentUserId()).Returns((string?)null);

        var result = service.GetCurrentUserId();

        result.Should().StartWith("anon-");
        result.Should().HaveLength(17); // "anon-" + 12 characters
    }

    [Fact]
    public void GetCurrentUserId_NotAuthenticated_NoHttpContext_ReturnsDefaultAnonymous()
    {
        var service = CreateService(out var jwtUserService, null);
        jwtUserService.Setup(j => j.GetCurrentUserId()).Returns((string?)null);

        var result = service.GetCurrentUserId();

        result.Should().Be("anonymous-user");
    }

    [Fact]
    public void GetCurrentUserId_WithProvidedHttpContext_UsesProvidedContext()
    {
        var providedContext = CreateHttpContext("10.0.0.1", "Custom-Agent/1.0");
        var service = CreateService(out var jwtUserService, null); // No context in accessor
        jwtUserService.Setup(j => j.GetCurrentUserId()).Returns((string?)null);

        var result = service.GetCurrentUserId(providedContext);

        result.Should().StartWith("anon-");
    }

    [Fact]
    public void GetCurrentUserId_EmptyJwtUserId_FallsBackToAnonymous()
    {
        var httpContext = CreateHttpContext("127.0.0.1", "TestAgent");
        var service = CreateService(out var jwtUserService, httpContext);
        jwtUserService.Setup(j => j.GetCurrentUserId()).Returns(string.Empty);

        var result = service.GetCurrentUserId();

        result.Should().StartWith("anon-");
    }

    [Fact]
    public void GetAnonymousUserId_SameIpAndUserAgent_ReturnsSameId()
    {
        var httpContext1 = CreateHttpContext("192.168.1.100", "Mozilla/5.0 Firefox");
        var httpContext2 = CreateHttpContext("192.168.1.100", "Mozilla/5.0 Firefox");
        var service = CreateService(out _);

        var result1 = service.GetAnonymousUserId(httpContext1);
        var result2 = service.GetAnonymousUserId(httpContext2);

        result1.Should().Be(result2);
    }

    [Fact]
    public void GetAnonymousUserId_DifferentIp_ReturnsDifferentId()
    {
        var httpContext1 = CreateHttpContext("192.168.1.1", "Mozilla/5.0");
        var httpContext2 = CreateHttpContext("192.168.1.2", "Mozilla/5.0");
        var service = CreateService(out _);

        var result1 = service.GetAnonymousUserId(httpContext1);
        var result2 = service.GetAnonymousUserId(httpContext2);

        result1.Should().NotBe(result2);
    }

    [Fact]
    public void GetAnonymousUserId_DifferentUserAgent_ReturnsDifferentId()
    {
        var httpContext1 = CreateHttpContext("192.168.1.1", "Mozilla/5.0 Firefox");
        var httpContext2 = CreateHttpContext("192.168.1.1", "Chrome/95.0");
        var service = CreateService(out _);

        var result1 = service.GetAnonymousUserId(httpContext1);
        var result2 = service.GetAnonymousUserId(httpContext2);

        result1.Should().NotBe(result2);
    }

    [Fact]
    public void GetAnonymousUserId_NullIpAddress_HandlesGracefully()
    {
        var httpContext = CreateHttpContext(null, "TestAgent");
        var service = CreateService(out _);

        var result = service.GetAnonymousUserId(httpContext);

        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
    }

    [Fact]
    public void GetAnonymousUserId_EmptyUserAgent_HandlesGracefully()
    {
        var httpContext = CreateHttpContext("192.168.1.1", string.Empty);
        var service = CreateService(out _);

        var result = service.GetAnonymousUserId(httpContext);

        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
    }

    [Fact]
    public void GetAnonymousUserId_NullUserAgent_HandlesGracefully()
    {
        var httpContext = CreateHttpContext("192.168.1.1", null);
        var service = CreateService(out _);

        var result = service.GetAnonymousUserId(httpContext);

        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
    }

    [Fact]
    public void GetAnonymousUserId_IPv6Address_WorksCorrectly()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("::1"); // IPv6 localhost
        context.Request.Headers.UserAgent = "TestAgent";
        
        var service = CreateService(out _);

        var result = service.GetAnonymousUserId(context);

        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
    }

    [Fact]
    public void GetAnonymousUserId_LongUserAgent_TruncatesCorrectly()
    {
        var longUserAgent = new string('X', 1000); // Very long user agent
        var httpContext = CreateHttpContext("192.168.1.1", longUserAgent);
        var service = CreateService(out _);

        var result = service.GetAnonymousUserId(httpContext);

        result.Should().StartWith("anon-");
        result.Should().HaveLength(17); // Should still be truncated to 12 chars + "anon-"
    }

    [Fact]
    public void GetAnonymousUserId_SpecialCharactersInUserAgent_HandlesCorrectly()
    {
        var userAgent = "Mozilla/5.0 (Windows NT; ±§!@#$%^&*()";
        var httpContext = CreateHttpContext("192.168.1.1", userAgent);
        var service = CreateService(out _);

        var result = service.GetAnonymousUserId(httpContext);

        result.Should().StartWith("anon-");
        result.Should().HaveLength(17);
        result.Should().MatchRegex(@"^anon-[A-Za-z0-9+/=]+$"); // Base64 pattern
    }

    [Fact]
    public void GetAnonymousUserId_ConsistentHashing_ProducesValidBase64()
    {
        var httpContext = CreateHttpContext("203.0.113.1", "Mozilla/5.0");
        var service = CreateService(out _);

        var result = service.GetAnonymousUserId(httpContext);

        var base64Part = result.Substring(5); // Remove "anon-" prefix
        base64Part.Should().HaveLength(12);
        
        // Should be valid base64 characters
        base64Part.Should().MatchRegex(@"^[A-Za-z0-9+/=]+$");
    }
}
