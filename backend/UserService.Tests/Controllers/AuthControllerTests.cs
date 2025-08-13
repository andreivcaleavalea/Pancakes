using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Controllers;
using UserService.Models.Requests;
using UserService.Services.Interfaces;

namespace UserService.Tests.Controllers;

public class AuthControllerTests
{
    private static AuthController Create(out Mock<IAuthService> svc)
    {
        svc = new Mock<IAuthService>(MockBehavior.Strict);
        var ctrl = new AuthController(svc.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return ctrl;
    }

    [Fact]
    public async Task Login_Delegates_To_Service()
    {
        var ctrl = Create(out var svc);
        svc.Setup(s => s.LoginAsync(ctrl.HttpContext, It.IsAny<LoginRequest>())).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.Login(new LoginRequest { Provider = "github", Code = "c" })).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void OAuthCallback_Delegates_To_Service()
    {
        var ctrl = Create(out var svc);
        svc.Setup(s => s.OAuthCallback("github", "code", "state")).Returns(new RedirectResult("/"));
        ctrl.OAuthCallback("github", "code", "state").Should().BeOfType<RedirectResult>();
    }

    [Fact]
    public void Logout_And_GetCurrentUser_And_Validate_Delegate()
    {
        var ctrl = Create(out var svc);
        svc.Setup(s => s.Logout(ctrl.HttpContext)).Returns(new OkObjectResult(new { }));
        ctrl.Logout().Should().BeOfType<OkObjectResult>();
        svc.Setup(s => s.GetCurrentUser(ctrl.HttpContext)).Returns(new UnauthorizedObjectResult(new { }));
        ctrl.GetCurrentUser().Should().BeOfType<UnauthorizedObjectResult>();
        svc.Setup(s => s.ValidateToken(ctrl.HttpContext)).Returns(new OkObjectResult(new { isValid = true }));
        ctrl.ValidateToken().Should().BeOfType<OkObjectResult>();
    }
}


