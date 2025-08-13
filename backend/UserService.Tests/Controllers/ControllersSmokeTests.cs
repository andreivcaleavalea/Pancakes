using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Controllers;
using UserService.Services.Interfaces;
using UserService.Models.DTOs;

namespace UserService.Tests.Controllers;

public class ControllersSmokeTests
{
    [Fact]
    public async Task UsersController_Wires_To_Service()
    {
        var svc = new Mock<IUserService>(MockBehavior.Strict);
        var ctrl = new UsersController(svc.Object) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        svc.Setup(s => s.GetByIdAsync(ctrl.HttpContext, "u")).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.GetById("u")).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ProfileController_Smoke()
    {
        var profile = new Mock<IProfileService>(MockBehavior.Strict);
        var edu = new Mock<IEducationService>(MockBehavior.Strict);
        var job = new Mock<IJobService>(MockBehavior.Strict);
        var hobby = new Mock<IHobbyService>(MockBehavior.Strict);
        var proj = new Mock<IProjectService>(MockBehavior.Strict);
        var current = new Mock<ICurrentUserService>(MockBehavior.Strict);
        var file = new Mock<IFileService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<ProfileController>>().Object;
        var ctrl = new ProfileController(profile.Object, edu.Object, job.Object, hobby.Object, proj.Object, current.Object, file.Object, logger)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        profile.Setup(p => p.GetProfileDataAsync("u1")).ReturnsAsync(new ProfileDataDto { User = new UserProfileDto() });
        (await ctrl.GetProfile()).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AssetsController_Serves_Or_404()
    {
        var file = new Mock<IFileService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AssetsController>>().Object;
        var ctrl = new AssetsController(file.Object, logger) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        file.Setup(f => f.GetProfilePictureAsync(It.IsAny<string>())).ReturnsAsync(((byte[], string, string)?)null);
        (await ctrl.GetProfilePicture("x.jpg")).Should().BeOfType<NotFoundObjectResult>();
    }
}


