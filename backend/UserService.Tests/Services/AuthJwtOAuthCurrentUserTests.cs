using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UserService.Services.Implementations;
using UserService.Services.Interfaces;
using UserService.Models.Requests;
using UserService.Models.Authentication;
using UserService.Models.Entities;

namespace UserService.Tests.Services;

public class AuthJwtOAuthCurrentUserTests
{
    [Fact]
    public void JwtService_Generate_And_Validate_Roundtrip()
    {
        var config = new ConfigurationBuilder().Build();
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", new string('a', 32));
        var svc = new JwtService(config);
        var user = new User { Id = "u1", Name = "A", Email = "a@x.com", Image = "i", Provider = "p", ProviderUserId = "pid", CreatedAt = DateTime.UtcNow, LastLoginAt = DateTime.UtcNow };
        var token = svc.GenerateToken(user);
        svc.ValidateToken(token).Should().BeTrue();
        var decoded = svc.DecodeToken(token);
        decoded!.Id.Should().Be("u1");
    }

    [Fact]
    public void CurrentUserService_Extracts_From_HttpContext()
    {
        var accessor = new HttpContextAccessor();
        var context = new DefaultHttpContext();
        accessor.HttpContext = context;
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "u1"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "A"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "a@x.com")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
        var svc = new CurrentUserService(accessor);
        svc.GetCurrentUserId().Should().Be("u1");
        svc.IsAuthenticated().Should().BeTrue();
        svc.GetCurrentUser()!.Email.Should().Be("a@x.com");
    }

    [Fact]
    public async Task AuthService_Login_OAuth_Success_And_Banned()
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

        var oauthInfo = new OAuthUserInfo { Id = "pid", Name = "A", Email = "a@x.com" };
        oauth.Setup(o => o.ExchangeCodeForUserInfo("code", "github")).ReturnsAsync(oauthInfo);
        var dto = new UserService.Models.DTOs.UserDto { Id = "u1", Name = "A", Email = "a@x.com" };
        userSvc.Setup(u => u.CreateOrUpdateFromOAuthAsync(oauthInfo, "github")).ReturnsAsync(dto);
        banSvc.Setup(b => b.ProcessExpiredBansAsync()).Returns(Task.CompletedTask);
        banSvc.Setup(b => b.GetActiveBanAsync("u1")).ReturnsAsync((UserService.Models.DTOs.BanDto?)null);
        mapperSvc.Setup(m => m.MapUserDtoToUser(dto)).Returns(new User { Id = "u1", Name = "A", Email = "a@x.com" });
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");

        var ok = await auth.LoginAsync(http, new LoginRequest { Provider = "github", Code = "code" });
        ok.Should().BeOfType<OkObjectResult>();

        // Banned path
        banSvc.Reset();
        oauth.Setup(o => o.ExchangeCodeForUserInfo("code", "github")).ReturnsAsync(oauthInfo);
        userSvc.Setup(u => u.CreateOrUpdateFromOAuthAsync(oauthInfo, "github")).ReturnsAsync(dto);
        banSvc.Setup(b => b.ProcessExpiredBansAsync()).Returns(Task.CompletedTask);
        banSvc.Setup(b => b.GetActiveBanAsync("u1")).ReturnsAsync(new UserService.Models.DTOs.BanDto { Reason = "x" });
        var unauthorized = await auth.LoginAsync(http, new LoginRequest { Provider = "github", Code = "code" });
        unauthorized.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task OAuthService_Token_Parse_And_UserInfo_Parse()
    {
        var handler = new DelegatingHandlerStub(async (req, ct) =>
        {
            await Task.Yield();
            if (req.RequestUri!.AbsoluteUri.Contains("/user"))
            {
                var json = "{\"id\":123,\"login\":\"gh\"}";
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(json) };
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("access_token=abc")
            };
        });
        var http = new HttpClient(handler);
        var config = new ConfigurationBuilder().Build();
        Environment.SetEnvironmentVariable("OAUTH_GITHUB_CLIENT_ID", "x");
        Environment.SetEnvironmentVariable("OAUTH_GITHUB_CLIENT_SECRET", "y");
        var oauth = new OAuthService(http, config);
        var token = await oauth.ExchangeCodeForToken("code", "github");
        token.Should().Be("abc");
        var info = await oauth.GetUserInfo("abc", "github");
        info.Should().NotBeNull();
        info!.Id.Should().Be("123");
    }

    private sealed class DelegatingHandlerStub : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;
        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        {
            _responder = responder;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _responder(request, cancellationToken);
        }
    }
}


