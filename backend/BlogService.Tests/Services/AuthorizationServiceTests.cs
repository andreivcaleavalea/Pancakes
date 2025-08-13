using BlogService.Models.DTOs;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BlogService.Tests.Services;

public class AuthorizationServiceTests
{
    private static AuthorizationService CreateService(out Mock<IUserServiceClient> client)
    {
        client = new Mock<IUserServiceClient>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthorizationService>>();
        return new AuthorizationService(client.Object, logger.Object);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Basic abc", "")]
    [InlineData("Bearer my-token", "my-token")]
    public void ExtractTokenFromHeader_Cases(string? header, string expected)
    {
        var service = CreateService(out _);
        var http = new DefaultHttpContext();
        if (header != null)
        {
            http.Request.Headers.Authorization = header;
        }

        var token = service.ExtractTokenFromHeader(http);
        token.Should().Be(expected);
    }

    [Fact]
    public async Task GetCurrentUserAsync_With_Token_Returns_User()
    {
        var service = CreateService(out var client);
        var http = new DefaultHttpContext();
        http.Request.Headers.Authorization = "Bearer t1";
        var user = new UserInfoDto { Id = "u1", Name = "User" };
        client.Setup(c => c.GetCurrentUserAsync("t1")).ReturnsAsync(user);

        var result = await service.GetCurrentUserAsync(http);
        result.Should().NotBeNull();
        result!.Id.Should().Be("u1");
    }

    [Fact]
    public async Task GetCurrentUserAsync_No_Token_Returns_Null()
    {
        var service = CreateService(out var client);
        var http = new DefaultHttpContext();

        var result = await service.GetCurrentUserAsync(http);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_Client_Returns_Null()
    {
        var service = CreateService(out var client);
        var http = new DefaultHttpContext();
        http.Request.Headers.Authorization = "Bearer t2";
        client.Setup(c => c.GetCurrentUserAsync("t2")).ReturnsAsync((UserInfoDto?)null);

        var result = await service.GetCurrentUserAsync(http);
        result.Should().BeNull();
    }
}


