using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Services.Implementations;
using UserService.Services.Interfaces;
using UserService.Models.Requests;
using UserService.Models.Authentication;
using UserService.Models.Entities;
using UserService.Models.DTOs;

namespace UserService.Tests.Services;

/// <summary>
/// Additional AuthService tests targeting previously uncovered branches / catch blocks
/// to push overall line coverage above the 80% requirement.
/// </summary>
public class AuthServiceAdditionalTests
{
    private AuthService BuildService(
        Mock<IOAuthService>? oauth = null,
        Mock<IJwtService>? jwt = null,
        Mock<ICurrentUserService>? current = null,
        Mock<IUserService>? userSvc = null,
        Mock<IUserMappingService>? mapper = null,
        Mock<IBanService>? ban = null)
    {
        oauth ??= new Mock<IOAuthService>();
        jwt ??= new Mock<IJwtService>();
        current ??= new Mock<ICurrentUserService>();
        userSvc ??= new Mock<IUserService>();
        mapper ??= new Mock<IUserMappingService>();
        ban ??= new Mock<IBanService>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthService>>().Object;
        return new AuthService(oauth.Object, jwt.Object, current.Object, userSvc.Object, mapper.Object, ban.Object, logger);
    }

    [Fact]
    public async Task LoginAsync_Returns_Unauthorized_With_Ban_That_Expires()
    {
        var oauthInfo = new OAuthUserInfo { Id = "pid", Name = "N", Email = "n@x.com" };
        var dto = new UserDto { Id = "u1", Name = "N", Email = "n@x.com" };
        var oauth = new Mock<IOAuthService>();
        var userSvc = new Mock<IUserService>();
        var ban = new Mock<IBanService>();
        var mapper = new Mock<IUserMappingService>();
        var jwt = new Mock<IJwtService>();
        oauth.Setup(o => o.ExchangeCodeForUserInfo("code", "github")).ReturnsAsync(oauthInfo);
        userSvc.Setup(u => u.CreateOrUpdateFromOAuthAsync(oauthInfo, "github")).ReturnsAsync(dto);
        mapper.Setup(m => m.MapUserDtoToUser(dto)).Returns(new User { Id = "u1", Name = "N", Email = "n@x.com" });
        ban.Setup(b => b.ProcessExpiredBansAsync()).Returns(Task.CompletedTask);
        // Ban with reason AND expiration (previously uncovered branch)
        ban.Setup(b => b.GetActiveBanAsync("u1")).ReturnsAsync(new BanDto { Reason = "Reason", ExpiresAt = DateTime.UtcNow.AddHours(1) });
        var svc = BuildService(oauth: oauth, userSvc: userSvc, mapper: mapper, ban: ban, jwt: jwt);
        var res = await svc.LoginAsync(new DefaultHttpContext(), new LoginRequest { Provider = "github", Code = "code" });
        res.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task LoginAsync_Returns_500_On_Exception()
    {
        var oauth = new Mock<IOAuthService>();
        // Force exception inside try block
        oauth.Setup(o => o.ExchangeCodeForUserInfo(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("boom"));
        var svc = BuildService(oauth: oauth);
        var res = await svc.LoginAsync(new DefaultHttpContext(), new LoginRequest { Provider = "github", Code = "code" });
        res.Should().BeOfType<ObjectResult>().Which.As<ObjectResult>().StatusCode.Should().Be(500);
    }

    [Fact]
    public void GetCurrentUser_Returns_Ok_When_User_Present()
    {
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.GetCurrentUser()).Returns(new User { Id = "u", Name = "User" });
        var svc = BuildService(current: current);
        var res = svc.GetCurrentUser(new DefaultHttpContext());
        res.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetCurrentUser_Returns_500_On_Exception()
    {
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.GetCurrentUser()).Throws(new Exception("err"));
        var svc = BuildService(current: current);
        var res = svc.GetCurrentUser(new DefaultHttpContext());
        res.Should().BeOfType<ObjectResult>().Which.As<ObjectResult>().StatusCode.Should().Be(500);
    }

    [Fact]
    public void Logout_Returns_500_On_Exception()
    {
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.GetCurrentUser()).Throws(new Exception("err"));
        var svc = BuildService(current: current);
        var res = svc.Logout(new DefaultHttpContext());
        res.Should().BeOfType<ObjectResult>().Which.As<ObjectResult>().StatusCode.Should().Be(500);
    }

    [Fact]
    public void ValidateToken_Returns_500_On_Exception()
    {
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.IsAuthenticated()).Throws(new Exception("err"));
        var svc = BuildService(current: current);
        var res = svc.ValidateToken(new DefaultHttpContext());
        res.Should().BeOfType<ObjectResult>().Which.As<ObjectResult>().StatusCode.Should().Be(500);
    }
}
