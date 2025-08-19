using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Controllers;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Tests.Controllers;

public class ProfileControllerTests
{
    private static ProfileController Create(
        out Mock<IProfileService> profile,
        out Mock<IEducationService> edu,
        out Mock<IJobService> job,
        out Mock<IHobbyService> hobby,
        out Mock<IProjectService> proj,
        out Mock<ICurrentUserService> current,
        out Mock<IFileService> file)
    {
        profile = new Mock<IProfileService>(MockBehavior.Strict);
        edu = new Mock<IEducationService>(MockBehavior.Strict);
        job = new Mock<IJobService>(MockBehavior.Strict);
        hobby = new Mock<IHobbyService>(MockBehavior.Strict);
        proj = new Mock<IProjectService>(MockBehavior.Strict);
        current = new Mock<ICurrentUserService>(MockBehavior.Strict);
        file = new Mock<IFileService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<ProfileController>>().Object;
        var ctrl = new ProfileController(profile.Object, edu.Object, job.Object, hobby.Object, proj.Object, current.Object, file.Object, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return ctrl;
    }

    [Fact]
    public async Task GetProfile_Unauthorized_When_No_User()
    {
        var ctrl = Create(out var profile, out var edu, out var job, out var hobby, out var proj, out var current, out var file);
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await ctrl.GetProfile()).Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetProfile_Ok_When_Found()
    {
        var ctrl = Create(out var profile, out var edu, out var job, out var hobby, out var proj, out var current, out var file);
        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        profile.Setup(p => p.GetProfileDataAsync("u1")).ReturnsAsync(new ProfileDataDto { User = new UserProfileDto() });
        (await ctrl.GetProfile()).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateUserProfile_BadRequest_Unauthorized_And_Ok()
    {
        var ctrl = Create(out var profile, out var edu, out var job, out var hobby, out var proj, out var current, out var file);
        ctrl.ModelState.AddModelError("x", "y");
        (await ctrl.UpdateUserProfile(new UpdateUserProfileDto())).Should().BeOfType<BadRequestObjectResult>();

        ctrl.ModelState.Clear();
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await ctrl.UpdateUserProfile(new UpdateUserProfileDto())).Should().BeOfType<UnauthorizedObjectResult>();

        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        profile.Setup(p => p.UpdateUserProfileAsync("u1", It.IsAny<UpdateUserProfileDto>())).ReturnsAsync(new UserProfileDto());
        (await ctrl.UpdateUserProfile(new UpdateUserProfileDto())).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateProfilePicture_NoFile_Unauthorized_And_Ok()
    {
        var ctrl = Create(out var profile, out var edu, out var job, out var hobby, out var proj, out var current, out var file);
        (await ctrl.UpdateProfilePicture(null!)).Should().BeOfType<BadRequestObjectResult>();

        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await ctrl.UpdateProfilePicture(new FormFile(Stream.Null, 0, 0, "f", "f.png") { Headers = new HeaderDictionary(), ContentType = "image/png" })).Should().BeOfType<UnauthorizedObjectResult>();

        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        profile.Setup(p => p.UpdateProfilePictureAsync("u1", It.IsAny<IFormFile>())).ReturnsAsync(new UserProfileDto());
        (await ctrl.UpdateProfilePicture(new FormFile(new MemoryStream(new byte[] { 1 }), 0, 1, "f", "f.png") { Headers = new HeaderDictionary(), ContentType = "image/png" })).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Educations_Jobs_Hobbies_Projects_Coverage()
    {
        var ctrl = Create(out var profile, out var edu, out var job, out var hobby, out var proj, out var current, out var file);
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await ctrl.GetEducations()).Should().BeOfType<UnauthorizedObjectResult>();

        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        edu.Setup(s => s.GetUserEducationsAsync("u1")).ReturnsAsync(new List<EducationDto>());
        (await ctrl.GetEducations()).Should().BeOfType<OkObjectResult>();

        ctrl.ModelState.AddModelError("x", "y");
        (await ctrl.AddEducation(new EducationDto())).Should().BeOfType<BadRequestObjectResult>();
        ctrl.ModelState.Clear();
        edu.Setup(s => s.CreateAsync("u1", It.IsAny<EducationDto>())).ReturnsAsync(new EducationDto());
        (await ctrl.AddEducation(new EducationDto())).Should().BeOfType<CreatedAtActionResult>();

        edu.Setup(s => s.UpdateAsync("u1", "e1", It.IsAny<EducationDto>())).ReturnsAsync(new EducationDto());
        (await ctrl.UpdateEducation("e1", new EducationDto())).Should().BeOfType<OkObjectResult>();
        edu.Setup(s => s.DeleteAsync("u1", "e2")).Returns(Task.CompletedTask);
        (await ctrl.DeleteEducation("e2")).Should().BeOfType<NoContentResult>();

        // Similar for Jobs
        job.Setup(s => s.GetUserJobsAsync("u1")).ReturnsAsync(new List<JobDto>());
        (await ctrl.GetJobs()).Should().BeOfType<OkObjectResult>();
        ctrl.ModelState.AddModelError("x", "y");
        (await ctrl.AddJob(new JobDto())).Should().BeOfType<BadRequestObjectResult>();
        ctrl.ModelState.Clear();
        job.Setup(s => s.CreateAsync("u1", It.IsAny<JobDto>())).ReturnsAsync(new JobDto());
        (await ctrl.AddJob(new JobDto())).Should().BeOfType<CreatedAtActionResult>();
        job.Setup(s => s.UpdateAsync("u1", "j1", It.IsAny<JobDto>())).ReturnsAsync(new JobDto());
        (await ctrl.UpdateJob("j1", new JobDto())).Should().BeOfType<OkObjectResult>();
        job.Setup(s => s.DeleteAsync("u1", "j2")).Returns(Task.CompletedTask);
        (await ctrl.DeleteJob("j2")).Should().BeOfType<NoContentResult>();

        // Hobbies
        hobby.Setup(s => s.GetUserHobbiesAsync("u1")).ReturnsAsync(new List<HobbyDto>());
        (await ctrl.GetHobbies()).Should().BeOfType<OkObjectResult>();
        ctrl.ModelState.AddModelError("x", "y");
        (await ctrl.AddHobby(new HobbyDto())).Should().BeOfType<BadRequestObjectResult>();
        ctrl.ModelState.Clear();
        hobby.Setup(s => s.CreateAsync("u1", It.IsAny<HobbyDto>())).ReturnsAsync(new HobbyDto());
        (await ctrl.AddHobby(new HobbyDto())).Should().BeOfType<CreatedAtActionResult>();
        hobby.Setup(s => s.UpdateAsync("u1", "h1", It.IsAny<HobbyDto>())).ReturnsAsync(new HobbyDto());
        (await ctrl.UpdateHobby("h1", new HobbyDto())).Should().BeOfType<OkObjectResult>();
        hobby.Setup(s => s.DeleteAsync("u1", "h2")).Returns(Task.CompletedTask);
        (await ctrl.DeleteHobby("h2")).Should().BeOfType<NoContentResult>();

        // Projects
        proj.Setup(s => s.GetUserProjectsAsync("u1")).ReturnsAsync(new List<ProjectDto>());
        (await ctrl.GetProjects()).Should().BeOfType<OkObjectResult>();
        ctrl.ModelState.AddModelError("x", "y");
        (await ctrl.AddProject(new ProjectDto())).Should().BeOfType<BadRequestObjectResult>();
        ctrl.ModelState.Clear();
        proj.Setup(s => s.CreateAsync("u1", It.IsAny<ProjectDto>())).ReturnsAsync(new ProjectDto());
        (await ctrl.AddProject(new ProjectDto())).Should().BeOfType<CreatedAtActionResult>();
        proj.Setup(s => s.UpdateAsync("u1", "p1", It.IsAny<ProjectDto>())).ReturnsAsync(new ProjectDto());
        (await ctrl.UpdateProject("p1", new ProjectDto())).Should().BeOfType<OkObjectResult>();
        proj.Setup(s => s.DeleteAsync("u1", "p2")).Returns(Task.CompletedTask);
        (await ctrl.DeleteProject("p2")).Should().BeOfType<NoContentResult>();
    }
}


