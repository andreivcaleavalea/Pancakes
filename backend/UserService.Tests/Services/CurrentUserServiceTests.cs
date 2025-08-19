using Microsoft.AspNetCore.Http;
using UserService.Services.Implementations;

namespace UserService.Tests.Services;

public class CurrentUserServiceTests
{
    [Fact]
    public void NoAuth_Returns_Nulls_And_False()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var svc = new CurrentUserService(accessor);
        svc.GetCurrentUser().Should().BeNull();
        svc.GetCurrentUserId().Should().BeNull();
        svc.GetUserEmail().Should().BeNull();
        svc.GetUserName().Should().BeNull();
        svc.IsAuthenticated().Should().BeFalse();
    }

    [Fact]
    public void MissingClaims_Still_Returns_User_With_Defaults()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var identity = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "u1")
        }, "TestAuth");
        accessor.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);
        var svc = new CurrentUserService(accessor);
        var user = svc.GetCurrentUser();
        user.Should().NotBeNull();
        user!.Id.Should().Be("u1");
    }
}


