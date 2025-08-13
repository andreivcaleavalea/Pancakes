using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Implementations;

namespace UserService.Tests.Repositories;

public class BanRepositoryTests
{
    private static UserDbContext Ctx()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase($"UserDb_{Guid.NewGuid()}")
            .Options;
        return new UserDbContext(options);
    }

    [Fact]
    public async Task BanRepository_Covers_All_Methods()
    {
        using var ctx = Ctx();
        var repo = new BanRepository(ctx);

        // Seed user
        ctx.Users.Add(new User { Id = "u1", Name = "A", Email = "a@x.com", Provider = "github", ProviderUserId = "u1" });
        await ctx.SaveChangesAsync();

        // Create
        var ban = await repo.CreateAsync(new Ban { UserId = "u1", Reason = "x", BannedBy = "admin", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        (await repo.GetByIdAsync(ban.Id))!.UserId.Should().Be("u1");

        // Active ban
        (await repo.GetActiveBanAsync("u1"))!.Id.Should().Be(ban.Id);
        (await repo.HasActiveBanAsync("u1")).Should().BeTrue();
        (await repo.GetActiveBansCountAsync()).Should().Be(1);
        (await repo.GetBanHistoryAsync("u1")).Should().HaveCount(1);

        // Update
        ban.Reason = "y";
        (await repo.UpdateAsync(ban)).Reason.Should().Be("y");

        // All active bans list (before expiration)
        (await repo.GetAllActiveBansAsync()).Should().NotBeEmpty();

        // Expiring (after setting cutoff at now, it may drop from active list)
        ban.ExpiresAt = DateTime.UtcNow;
        await repo.UpdateAsync(ban);
        (await repo.GetExpiringBansAsync(DateTime.UtcNow)).Should().NotBeEmpty();

        // Delete
        await repo.DeleteAsync(ban.Id);
        (await repo.GetByIdAsync(ban.Id)).Should().BeNull();
    }
}


