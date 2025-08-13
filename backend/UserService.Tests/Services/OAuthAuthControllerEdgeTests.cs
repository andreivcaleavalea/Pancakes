using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UserService.Services.Implementations;
using UserService.Services.Interfaces;
using UserService.Models.Requests;
using UserService.Models.Authentication;
using UserService.Models.Entities;

namespace UserService.Tests.Services;

public class OAuthAuthControllerEdgeTests
{
    [Fact]
    public async Task AuthService_Login_Returns_BadRequest_When_OAuth_Fails()
    {
        var oauth = new Mock<IOAuthService>();
        var jwt = new Mock<IJwtService>();
        var current = new Mock<ICurrentUserService>();
        var userSvc = new Mock<IUserService>();
        var mapperSvc = new Mock<IUserMappingService>();
        var banSvc = new Mock<IBanService>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthService>>().Object;
        oauth.Setup(o => o.ExchangeCodeForUserInfo("bad", "github")).ReturnsAsync((OAuthUserInfo?)null);
        var auth = new AuthService(oauth.Object, jwt.Object, current.Object, userSvc.Object, mapperSvc.Object, banSvc.Object, logger);
        var res = await auth.LoginAsync(new DefaultHttpContext(), new LoginRequest { Code = "bad", Provider = "github" });
        res.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void AuthService_Callback_And_Logout_And_GetCurrentUser_And_ValidateToken()
    {
        var oauth = new Mock<IOAuthService>();
        var jwt = new Mock<IJwtService>();
        var current = new Mock<ICurrentUserService>();
        var userSvc = new Mock<IUserService>();
        var mapperSvc = new Mock<IUserMappingService>();
        var banSvc = new Mock<IBanService>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthService>>().Object;
        var auth = new AuthService(oauth.Object, jwt.Object, current.Object, userSvc.Object, mapperSvc.Object, banSvc.Object, logger);
        var http = new DefaultHttpContext();

        // Callback returns redirect
        var r = auth.OAuthCallback("github", "code", "state");
        r.Should().BeOfType<RedirectResult>();

        // Logout
        current.Setup(c => c.GetCurrentUser()).Returns(new User { Id = "u", Name = "n" });
        auth.Logout(http).Should().BeOfType<OkObjectResult>();

        // GetCurrentUser unauthorized
        current.Setup(c => c.GetCurrentUser()).Returns((User?)null);
        auth.GetCurrentUser(http).Should().BeOfType<UnauthorizedObjectResult>();

        // ValidateToken wrapper
        current.Setup(c => c.IsAuthenticated()).Returns(true);
        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        auth.ValidateToken(http).Should().BeOfType<OkObjectResult>();
    }
}


