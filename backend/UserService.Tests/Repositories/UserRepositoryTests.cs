using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Implementations;

namespace UserService.Tests.Repositories;

public class UserRepositoryTests
{
    private static UserDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase($"UserDb_{Guid.NewGuid()}")
            .Options;
        return new UserDbContext(options);
    }

    private static User CreateUser(string id, string email, string name)
    {
        return new User
        {
            Id = id,
            Email = email,
            Name = name,
            Provider = "github",
            ProviderUserId = id
        };
    }

    [Fact]
    public async Task Crud_And_Queries_Work()
    {
        using var ctx = CreateContext();
        var repo = new UserRepository(ctx);

        // Create
        var u1 = await repo.CreateAsync(CreateUser("u1", "a@x.com", "Alice"));
        var u2 = await repo.CreateAsync(CreateUser("u2", "b@x.com", "Bob"));

        // GetById / Email / Provider
        (await repo.GetByIdAsync("u1"))!.Email.Should().Be("a@x.com");
        (await repo.GetByEmailAsync("b@x.com"))!.Id.Should().Be("u2");
        (await repo.GetByProviderAndProviderUserIdAsync("github", "u2"))!.Id.Should().Be("u2");

        // GetAll / Count / Search
        var all = await repo.GetAllAsync(1, 10);
        all.Count().Should().Be(2);
        var (paged, total) = await repo.GetAllWithCountAsync(1, 1);
        total.Should().Be(2);
        var (search, scount) = await repo.SearchUsersAsync("al", 1, 10);
        search.Any(u => u.Name == "Alice").Should().BeTrue();

        // Batch by ids
        var batch = await repo.GetUsersByIdsAsync(new[] { "u1", "u2", "ghost" });
        batch.Select(x => x.Id).Should().BeEquivalentTo(new[] { "u1", "u2" });

        // Exists checks
        (await repo.ExistsAsync("u1")).Should().BeTrue();
        (await repo.ExistsByEmailAsync("a@x.com")).Should().BeTrue();
        (await repo.ExistsByProviderAndProviderUserIdAsync("github", "u1")).Should().BeTrue();

        // Update
        u1.Name = "Alice2";
        (await repo.UpdateAsync(u1)).Name.Should().Be("Alice2");

        // Delete
        await repo.DeleteAsync("u2");
        (await repo.GetByIdAsync("u2")).Should().BeNull();
    }
}


