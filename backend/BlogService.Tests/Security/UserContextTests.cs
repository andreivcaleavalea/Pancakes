using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BlogService.Tests.Security;

public class UserContextTests
{
    private static string CreateJwt(string userId)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "0123456789abcdef0123456789abcdef"; // 32 bytes
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesBlog";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesBlogUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static JwtUserService CreateJwtUserService(HttpContext ctx)
    {
        var accessor = new HttpContextAccessor { HttpContext = ctx };
        return new JwtUserService(accessor);
    }

    [Fact]
    public void JwtUserService_No_Header_Returns_Null()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "0123456789abcdef0123456789abcdef");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "PancakesBlog");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "PancakesBlogUsers");

        var ctx = new DefaultHttpContext();
        var jwt = CreateJwtUserService(ctx);
        jwt.GetCurrentUserId().Should().BeNull();
    }

    [Fact]
    public void JwtUserService_Valid_Header_Returns_UserId()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "0123456789abcdef0123456789abcdef");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "PancakesBlog");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "PancakesBlogUsers");

        var ctx = new DefaultHttpContext();
        var userId = "user-123";
        var token = CreateJwt(userId);
        ctx.Request.Headers.Authorization = $"Bearer {token}";

        var jwt = CreateJwtUserService(ctx);
        jwt.GetCurrentUserId().Should().Be(userId);
    }

    [Fact]
    public void UserContextService_Uses_Jwt_Then_Anonymous_Id()
    {
        var ctx = new DefaultHttpContext();
        ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        ctx.Request.Headers.UserAgent = "TestAgent/1.0";

        var jwt = new Mock<IJwtUserService>();
        var accessor = new HttpContextAccessor { HttpContext = ctx };
        var logger = new Mock<ILogger<UserContextService>>();
        var svc = new UserContextService(jwt.Object, accessor, logger.Object);

        // When JWT has user
        jwt.Setup(j => j.GetCurrentUserId()).Returns("u1");
        svc.GetCurrentUserId().Should().Be("u1");

        // When JWT null -> anon id derived from IP+UA, should be stable
        jwt.Setup(j => j.GetCurrentUserId()).Returns((string?)null);
        var anon1 = svc.GetCurrentUserId(ctx);
        var anon2 = svc.GetCurrentUserId(ctx);
        anon1.Should().StartWith("anon-");
        anon2.Should().Be(anon1);
    }
}


