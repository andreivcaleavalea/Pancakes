using UserService.Services.Implementations;
using UserService.Repositories.Interfaces;
using UserService.Models.Entities;
using UserService.Models.DTOs;
using UserService.Helpers;
using UserService.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace UserService.Tests.Services;

public class ProfileServiceTests
{
    private static ProfileService CreateService(out Mock<IUserRepository> users,
        out Mock<IEducationRepository> edu,
        out Mock<IJobRepository> jobs,
        out Mock<IHobbyRepository> hobbies,
        out Mock<IProjectRepository> projects,
        out Mock<IFileService> files,
        out Mock<IProfilePictureStrategyFactory> picFactory,
        out Mock<IProfilePictureStrategy> strategy)
    {
        users = new Mock<IUserRepository>(MockBehavior.Strict);
        edu = new Mock<IEducationRepository>(MockBehavior.Strict);
        jobs = new Mock<IJobRepository>(MockBehavior.Strict);
        hobbies = new Mock<IHobbyRepository>(MockBehavior.Strict);
        projects = new Mock<IProjectRepository>(MockBehavior.Strict);
        files = new Mock<IFileService>(MockBehavior.Strict);
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
        picFactory = new Mock<IProfilePictureStrategyFactory>(MockBehavior.Strict);
        strategy = new Mock<IProfilePictureStrategy>(MockBehavior.Strict);
        picFactory.Setup(f => f.GetStrategyForUserUpload()).Returns(strategy.Object);
        return new ProfileService(users.Object, edu.Object, jobs.Object, hobbies.Object, projects.Object, files.Object, mapper, picFactory.Object);
    }

    [Fact]
    public async Task GetProfileData_Returns_Composite_When_User_Exists()
    {
        var s = CreateService(out var users, out var edu, out var jobs, out var hobbies, out var projects, out var files, out var picFactory, out var strategy);
        users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1", Name = "A" });
        edu.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Education>());
        jobs.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Job>());
        hobbies.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Hobby>());
        projects.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Project>());

        var res = await s.GetProfileDataAsync("u1");
        res.Should().NotBeNull();
        res!.User.Name.Should().Be("A");
    }

    [Fact]
    public async Task GetProfileData_Returns_Null_When_User_Missing()
    {
        var s = CreateService(out var users, out var edu, out var jobs, out var hobbies, out var projects, out var files, out var picFactory, out var strategy);
        users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((User?)null);
        var res = await s.GetProfileDataAsync("u1");
        res.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserProfile_Parses_Date_And_Maps()
    {
        var s = CreateService(out var users, out var edu, out var jobs, out var hobbies, out var projects, out var files, out var picFactory, out var strategy);
        var user = new User { Id = "u1", Name = "Old" };
        users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);
        users.Setup(r => r.UpdateAsync(user)).ReturnsAsync(user);

        var dto = new UpdateUserProfileDto { Name = "New", DateOfBirth = "2000-01-02" };
        var result = await s.UpdateUserProfileAsync("u1", dto);
        result.Name.Should().Be("New");
    }

    [Fact]
    public async Task UpdateProfilePicture_Saves_File_And_Applies_Strategy()
    {
        var s = CreateService(out var users, out var edu, out var jobs, out var hobbies, out var projects, out var files, out var picFactory, out var strategy);
        var user = new User { Id = "u1", Image = "old.png" };
        users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);
        files.Setup(f => f.SaveProfilePictureAsync(It.IsAny<IFormFile>(), "u1")).ReturnsAsync("assets/profile-pictures/new.png");
        strategy.Setup(str => str.HandleUserUpload(user, It.IsAny<string>())).Callback<User, string>((u, path) => u.Image = path);
        users.Setup(r => r.UpdateAsync(user)).ReturnsAsync(user);

        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns("x.png");
        formFile.Setup(f => f.Length).Returns(10);
        using var ms = new MemoryStream(new byte[10]);
        formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var dto = await s.UpdateProfilePictureAsync("u1", formFile.Object);
        dto.Avatar.Should().Contain("new.png");
    }
}


