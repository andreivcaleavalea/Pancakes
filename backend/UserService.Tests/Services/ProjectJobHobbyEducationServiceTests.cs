using UserService.Services.Implementations;
using UserService.Repositories.Interfaces;
using UserService.Models.Entities;
using UserService.Models.DTOs;
using UserService.Helpers;

namespace UserService.Tests.Services;

public class ProjectJobHobbyEducationServiceTests
{
    private static ProjectService CreateProject(out Mock<IProjectRepository> repo, out Mock<IUserRepository> users)
    {
        repo = new Mock<IProjectRepository>(MockBehavior.Strict);
        users = new Mock<IUserRepository>(MockBehavior.Strict);
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
        return new ProjectService(repo.Object, users.Object, mapper);
    }

    private static JobService CreateJob(out Mock<IJobRepository> repo, out Mock<IUserRepository> users)
    {
        repo = new Mock<IJobRepository>(MockBehavior.Strict);
        users = new Mock<IUserRepository>(MockBehavior.Strict);
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
        return new JobService(repo.Object, users.Object, mapper);
    }

    private static HobbyService CreateHobby(out Mock<IHobbyRepository> repo, out Mock<IUserRepository> users)
    {
        repo = new Mock<IHobbyRepository>(MockBehavior.Strict);
        users = new Mock<IUserRepository>(MockBehavior.Strict);
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
        return new HobbyService(repo.Object, users.Object, mapper);
    }

    private static EducationService CreateEducation(out Mock<IEducationRepository> repo, out Mock<IUserRepository> users)
    {
        repo = new Mock<IEducationRepository>(MockBehavior.Strict);
        users = new Mock<IUserRepository>(MockBehavior.Strict);
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
        return new EducationService(repo.Object, users.Object, mapper);
    }

    [Fact]
    public async Task ProjectService_CRUD()
    {
        var s = CreateProject(out var repo, out var users);
        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Project>());
        (await s.GetUserProjectsAsync("u1")).Should().NotBeNull();

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.CreateAsync(It.IsAny<Project>())).ReturnsAsync((Project p) => p);
        (await s.CreateAsync("u1", new ProjectDto { Name = "t", Description = "descdescdesc", Technologies = "c#", StartDate = "2024-01" })).Name.Should().Be("t");

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync("p1")).ReturnsAsync(new Project { Id = "p1", UserId = "u1" });
        repo.Setup(r => r.UpdateAsync(It.IsAny<Project>())).ReturnsAsync((Project p) => p);
        (await s.UpdateAsync("u1", "p1", new ProjectDto { Name = "n", Description = "descdescdesc", Technologies = "c#", StartDate = "2024-01" })).Name.Should().Be("n");

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync("p1")).ReturnsAsync(new Project { Id = "p1", UserId = "u1" });
        repo.Setup(r => r.DeleteAsync("p1")).Returns(Task.CompletedTask);
        await s.DeleteAsync("u1", "p1");
    }

    [Fact]
    public async Task JobService_CRUD()
    {
        var s = CreateJob(out var repo, out var users);
        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Job>());
        (await s.GetUserJobsAsync("u1")).Should().NotBeNull();

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.CreateAsync(It.IsAny<Job>())).ReturnsAsync((Job p) => p);
        (await s.CreateAsync("u1", new JobDto { Company = "c" })).Company.Should().Be("c");

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync("j1")).ReturnsAsync(new Job { Id = "j1", UserId = "u1" });
        repo.Setup(r => r.UpdateAsync(It.IsAny<Job>())).ReturnsAsync((Job p) => p);
        (await s.UpdateAsync("u1", "j1", new JobDto { Company = "n" })).Company.Should().Be("n");

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync("j1")).ReturnsAsync(new Job { Id = "j1", UserId = "u1" });
        repo.Setup(r => r.DeleteAsync("j1")).Returns(Task.CompletedTask);
        await s.DeleteAsync("u1", "j1");
    }

    [Fact]
    public async Task HobbyService_CRUD()
    {
        var s = CreateHobby(out var repo, out var users);
        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Hobby>());
        (await s.GetUserHobbiesAsync("u1")).Should().NotBeNull();

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.CreateAsync(It.IsAny<Hobby>())).ReturnsAsync((Hobby p) => p);
        (await s.CreateAsync("u1", new HobbyDto { Name = "h" })).Name.Should().Be("h");

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync("h1")).ReturnsAsync(new Hobby { Id = "h1", UserId = "u1" });
        repo.Setup(r => r.UpdateAsync(It.IsAny<Hobby>())).ReturnsAsync((Hobby p) => p);
        (await s.UpdateAsync("u1", "h1", new HobbyDto { Name = "n" })).Name.Should().Be("n");

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync("h1")).ReturnsAsync(new Hobby { Id = "h1", UserId = "u1" });
        repo.Setup(r => r.DeleteAsync("h1")).Returns(Task.CompletedTask);
        await s.DeleteAsync("u1", "h1");
    }

    [Fact]
    public async Task EducationService_CRUD()
    {
        var s = CreateEducation(out var repo, out var users);
        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Education>());
        (await s.GetUserEducationsAsync("u1")).Should().NotBeNull();

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.CreateAsync(It.IsAny<Education>())).ReturnsAsync((Education p) => p);
        (await s.CreateAsync("u1", new EducationDto { Institution = "s", Specialization = "sp", Degree = "deg", StartDate = "2020-01" })).Institution.Should().Be("s");

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync("e1")).ReturnsAsync(new Education { Id = "e1", UserId = "u1" });
        repo.Setup(r => r.UpdateAsync(It.IsAny<Education>())).ReturnsAsync((Education p) => p);
        (await s.UpdateAsync("u1", "e1", new EducationDto { Institution = "n", Specialization = "sp", Degree = "deg", StartDate = "2020-01" })).Institution.Should().Be("n");

        users.Setup(u => u.ExistsAsync("u1")).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync("e1")).ReturnsAsync(new Education { Id = "e1", UserId = "u1" });
        repo.Setup(r => r.DeleteAsync("e1")).Returns(Task.CompletedTask);
        await s.DeleteAsync("u1", "e1");
    }
}


