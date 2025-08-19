using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Services.Implementations;
using UserService.Repositories.Interfaces;
using UserService.Models.Entities;
using UserService.Models.DTOs;
using UserService.Models.Requests;
using UserService.Helpers;

namespace UserService.Tests.Services;

public class BanServiceTests
{
    private static BanService CreateService(out Mock<IBanRepository> bans, out Mock<IUserRepository> users)
    {
        bans = new Mock<IBanRepository>(MockBehavior.Strict);
        users = new Mock<IUserRepository>(MockBehavior.Strict);
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<BanService>>().Object;
        return new BanService(bans.Object, users.Object, mapper, logger);
    }

    [Fact]
    public async Task Basic_Ban_Queries_Map()
    {
        var s = CreateService(out var bans, out var users);
        var ban = new Ban { Id = "b1", UserId = "u1", IsActive = true };
        bans.Setup(r => r.GetActiveBanAsync("u1")).ReturnsAsync(ban);
        (await s.GetActiveBanAsync("u1"))!.Id.Should().Be("b1");

        bans.Setup(r => r.GetBanHistoryAsync("u1")).ReturnsAsync(new List<Ban> { ban });
        (await s.GetBanHistoryAsync("u1")).Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_And_Lift_Ban()
    {
        var s = CreateService(out var bans, out var users);
        var req = new BanUserRequest { UserId = "u1", Reason = "x", BannedBy = "admin" };
        bans.Setup(r => r.CreateAsync(It.IsAny<Ban>())).ReturnsAsync((Ban b) => b);
        var dto = await s.CreateBanAsync(req);
        dto.IsActive.Should().BeTrue();

        var existing = new Ban { Id = "b1", UserId = "u1", IsActive = true };
        bans.Setup(r => r.GetByIdAsync("b1")).ReturnsAsync(existing);
        bans.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);
        var lifted = await s.LiftBanAsync("b1", "mod", "ok");
        lifted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessExpiredBans_Updates_All()
    {
        var s = CreateService(out var bans, out var users);
        var exp = new List<Ban> { new Ban { Id = "b1", UserId = "u1", IsActive = true } };
        bans.Setup(r => r.GetExpiringBansAsync(It.IsAny<DateTime>())).ReturnsAsync(exp);
        bans.Setup(r => r.UpdateAsync(It.IsAny<Ban>())).ReturnsAsync((Ban b) => b);
        await s.ProcessExpiredBansAsync();
    }

    [Fact]
    public async Task Wrappers_Ban_Unban_History_Paths()
    {
        var s = CreateService(out var bans, out var users);
        var http = new DefaultHttpContext();

        // BanUser: user not found -> NotFound
        users.Setup(u => u.GetByIdAsync("u1")).ReturnsAsync((User?)null);
        var nf = await s.BanUserAsync(http, new BanUserRequest { UserId = "u1" });
        nf.Should().BeOfType<NotFoundObjectResult>();

        // BanUser: already banned -> BadRequest
        users.Setup(u => u.GetByIdAsync("u2")).ReturnsAsync(new User { Id = "u2" });
        bans.Setup(b => b.HasActiveBanAsync("u2")).ReturnsAsync(true);
        var br = await s.BanUserAsync(http, new BanUserRequest { UserId = "u2" });
        br.Should().BeOfType<BadRequestObjectResult>();

        // BanUser: success -> Ok
        users.Setup(u => u.GetByIdAsync("u3")).ReturnsAsync(new User { Id = "u3" });
        bans.Setup(b => b.HasActiveBanAsync("u3")).ReturnsAsync(false);
        bans.Setup(r => r.CreateAsync(It.IsAny<Ban>())).ReturnsAsync((Ban b) => b);
        var ok = await s.BanUserAsync(http, new BanUserRequest { UserId = "u3" });
        ok.Should().BeOfType<OkObjectResult>();

        // Unban: active ban missing -> BadRequest
        bans.Setup(r => r.GetActiveBanAsync("u4")).ReturnsAsync((Ban?)null);
        var ubad = await s.UnbanUserAsync(http, new UnbanUserRequest { UserId = "u4" });
        ubad.Should().BeOfType<BadRequestObjectResult>();

        // Unban: success -> Ok
        var active = new Ban { Id = "b2", UserId = "u5", IsActive = true };
        bans.Setup(r => r.GetActiveBanAsync("u5")).ReturnsAsync(active);
        bans.Setup(r => r.GetByIdAsync("b2")).ReturnsAsync(active);
        bans.Setup(r => r.UpdateAsync(active)).ReturnsAsync(active);
        var uok = await s.UnbanUserAsync(http, new UnbanUserRequest { UserId = "u5", UnbannedBy = "mod", Reason = "x" });
        uok.Should().BeOfType<OkObjectResult>();

        // History: user not found -> NotFound
        users.Setup(u => u.GetByIdAsync("u6")).ReturnsAsync((User?)null);
        var hnf = await s.GetBanHistoryAsync(http, "u6");
        hnf.Should().BeOfType<NotFoundObjectResult>();

        // History: ok
        users.Setup(u => u.GetByIdAsync("u7")).ReturnsAsync(new User { Id = "u7" });
        bans.Setup(r => r.GetBanHistoryAsync("u7")).ReturnsAsync(new List<Ban>());
        var hok = await s.GetBanHistoryAsync(http, "u7");
        hok.Should().BeOfType<OkObjectResult>();
    }
}


