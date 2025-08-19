using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Implementations;

namespace UserService.Tests.Repositories;

public class OtherRepositoriesTests
{
    private static UserDbContext Ctx()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase($"UserDb_{Guid.NewGuid()}")
            .Options;
        return new UserDbContext(options);
    }

    [Fact]
    public async Task EducationRepository_CRUD()
    {
        using var ctx = Ctx();
        var repo = new EducationRepository(ctx);
        var ed = await repo.CreateAsync(new Education { UserId = "u", Institution = "inst", Degree = "deg", Specialization = "sp", StartDate = "2020-01" });
        (await repo.GetByIdAsync(ed.Id))!.UserId.Should().Be("u");
        (await repo.GetByUserIdAsync("u")).Should().HaveCount(1);
        ed.Description = "d";
        (await repo.UpdateAsync(ed)).Description.Should().Be("d");
        (await repo.ExistsAsync(ed.Id)).Should().BeTrue();
        await repo.DeleteAsync(ed.Id);
        (await repo.GetByIdAsync(ed.Id)).Should().BeNull();
    }

    [Fact]
    public async Task JobRepository_CRUD()
    {
        using var ctx = Ctx();
        var repo = new JobRepository(ctx);
        var job = await repo.CreateAsync(new Job { UserId = "u", Company = "c", Position = "p", StartDate = "2020-01" });
        (await repo.GetByIdAsync(job.Id))!.Company.Should().Be("c");
        (await repo.GetByUserIdAsync("u")).Should().HaveCount(1);
        job.Location = "L";
        (await repo.UpdateAsync(job)).Location.Should().Be("L");
        (await repo.ExistsAsync(job.Id)).Should().BeTrue();
        await repo.DeleteAsync(job.Id);
        (await repo.GetByIdAsync(job.Id)).Should().BeNull();
    }

    [Fact]
    public async Task HobbyRepository_CRUD()
    {
        using var ctx = Ctx();
        var repo = new HobbyRepository(ctx);
        var hobby = await repo.CreateAsync(new Hobby { UserId = "u", Name = "h" });
        (await repo.GetByIdAsync(hobby.Id))!.Name.Should().Be("h");
        (await repo.GetByUserIdAsync("u")).Should().HaveCount(1);
        hobby.Description = "d";
        (await repo.UpdateAsync(hobby)).Description.Should().Be("d");
        (await repo.ExistsAsync(hobby.Id)).Should().BeTrue();
        await repo.DeleteAsync(hobby.Id);
        (await repo.GetByIdAsync(hobby.Id)).Should().BeNull();
    }

    [Fact]
    public async Task ProjectRepository_CRUD()
    {
        using var ctx = Ctx();
        var repo = new ProjectRepository(ctx);
        var proj = await repo.CreateAsync(new Project { UserId = "u", Name = "n", Description = "descdescdesc", Technologies = "c#", StartDate = "2020-01" });
        (await repo.GetByIdAsync(proj.Id))!.Name.Should().Be("n");
        (await repo.GetByUserIdAsync("u")).Should().HaveCount(1);
        proj.Description = "d";
        (await repo.UpdateAsync(proj)).Description.Should().Be("d");
        (await repo.ExistsAsync(proj.Id)).Should().BeTrue();
        await repo.DeleteAsync(proj.Id);
        (await repo.GetByIdAsync(proj.Id)).Should().BeNull();
    }

    [Fact]
    public async Task FriendshipRepository_CRUD_And_Queries()
    {
        using var ctx = Ctx();
        var repo = new FriendshipRepository(ctx);
        var fr = await repo.CreateAsync(new Friendship { SenderId = "a", ReceiverId = "b", Status = FriendshipStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        (await repo.GetByIdAsync(fr.Id))!.SenderId.Should().Be("a");
        (await repo.GetFriendshipAsync("a", "b"))!.ReceiverId.Should().Be("b");
        (await repo.GetPendingRequestsSentAsync("a")).Should().HaveCount(1);
        (await repo.GetPendingRequestsReceivedAsync("b")).Should().HaveCount(1);
        fr.Status = FriendshipStatus.Accepted;
        (await repo.UpdateAsync(fr)).Status.Should().Be(FriendshipStatus.Accepted);
        (await repo.GetUserFriendsAsync("a")).Should().NotBeEmpty();
        (await repo.GetFriendUserIdsAsync("a")).Should().Contain("b");
        (await repo.AreFriendsAsync("a","b")).Should().BeTrue();
        await repo.DeleteAsync(fr.Id);
        (await repo.GetByIdAsync(fr.Id)).Should().BeNull();
    }
}


