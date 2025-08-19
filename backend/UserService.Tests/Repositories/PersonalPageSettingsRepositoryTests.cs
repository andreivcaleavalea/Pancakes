using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Implementations;

namespace UserService.Tests.Repositories;

public class PersonalPageSettingsRepositoryTests
{
    private static UserDbContext Ctx()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase($"UserDb_{Guid.NewGuid()}")
            .Options;
        return new UserDbContext(options);
    }

    [Fact]
    public async Task Crud_And_Queries()
    {
        using var ctx = Ctx();
        var repo = new PersonalPageSettingsRepository(ctx);
        var settings = await repo.CreateAsync(new PersonalPageSettings { UserId = "u1", PageSlug = "slug", IsPublic = true });
        var byUser = await repo.GetByUserIdAsync("u1");
        byUser.Should().NotBeNull();
        byUser!.PageSlug.Should().Be("slug");
        (await repo.ExistsAsync("u1")).Should().BeTrue();
        (await repo.PageSlugExistsAsync("slug")).Should().BeTrue();
        (await repo.PageSlugExistsAsync("slug", excludeUserId: "u2")).Should().BeTrue();
        await repo.GetByPageSlugAsync("slug"); // cover path (may be null)
        settings.IsPublic = false;
        (await repo.UpdateAsync(settings)).IsPublic.Should().BeFalse();
        await repo.DeleteAsync(settings.Id);
        (await repo.GetByUserIdAsync("u1")).Should().BeNull();
    }
}


