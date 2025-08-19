using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Controllers;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Tests.Controllers;

public class PersonalPageControllerTests
{
    private static PersonalPageController Create(out Mock<IPersonalPageService> svc, out Mock<ICurrentUserService> current)
    {
        svc = new Mock<IPersonalPageService>(MockBehavior.Strict);
        current = new Mock<ICurrentUserService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<PersonalPageController>>().Object;
        var ctrl = new PersonalPageController(svc.Object, current.Object, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return ctrl;
    }

    [Fact]
    public async Task GetSettings_Unauthorized_And_Ok()
    {
        var ctrl = Create(out var svc, out var current);
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await ctrl.GetSettings()).Should().BeOfType<UnauthorizedObjectResult>();

        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        svc.Setup(s => s.GetSettingsAsync("u1")).ReturnsAsync(new PersonalPageSettingsDto());
        (await ctrl.GetSettings()).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateSettings_Unauthorized_BadRequest_And_Ok()
    {
        var ctrl = Create(out var svc, out var current);
        ctrl.ModelState.AddModelError("x", "y");
        (await ctrl.UpdateSettings(new UpdatePersonalPageSettingsDto())).Should().BeOfType<BadRequestObjectResult>();

        ctrl.ModelState.Clear();
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await ctrl.UpdateSettings(new UpdatePersonalPageSettingsDto())).Should().BeOfType<UnauthorizedObjectResult>();

        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        svc.Setup(s => s.UpdateSettingsAsync("u1", It.IsAny<UpdatePersonalPageSettingsDto>())).ReturnsAsync(new PersonalPageSettingsDto());
        (await ctrl.UpdateSettings(new UpdatePersonalPageSettingsDto())).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPublicPage_NotFound_And_Ok()
    {
        var ctrl = Create(out var svc, out var current);
        svc.Setup(s => s.GetPublicPageAsync("slug")).ReturnsAsync((PublicPersonalPageDto?)null);
        (await ctrl.GetPublicPage("slug")).Should().BeOfType<NotFoundObjectResult>();

        svc.Setup(s => s.GetPublicPageAsync("slug")).ReturnsAsync(new PublicPersonalPageDto { User = new UserProfileDto() });
        (await ctrl.GetPublicPage("slug")).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GenerateSlug_Unauthorized_And_Ok()
    {
        var ctrl = Create(out var svc, out var current);
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await ctrl.GenerateSlug(new GenerateSlugRequest { BaseName = "x" })).Should().BeOfType<UnauthorizedObjectResult>();

        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        svc.Setup(s => s.GenerateUniqueSlugAsync("x", "u1")).ReturnsAsync("x-1");
        var res = await ctrl.GenerateSlug(new GenerateSlugRequest { BaseName = "x" }) as OkObjectResult;
        res.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPreview_Unauthorized_NotFound_And_Ok()
    {
        var ctrl = Create(out var svc, out var current);
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await ctrl.GetPreview()).Should().BeOfType<UnauthorizedObjectResult>();

        current.Setup(c => c.GetCurrentUserId()).Returns("u1");
        svc.Setup(s => s.GetSettingsAsync("u1")).ReturnsAsync(new PersonalPageSettingsDto { PageSlug = "" });
        (await ctrl.GetPreview()).Should().BeOfType<NotFoundObjectResult>();

        svc.Setup(s => s.GetSettingsAsync("u1")).ReturnsAsync(new PersonalPageSettingsDto { PageSlug = "slug" });
        svc.Setup(s => s.GetPublicPageAsync("slug")).ReturnsAsync(new PublicPersonalPageDto { User = new UserProfileDto() });
        (await ctrl.GetPreview()).Should().BeOfType<OkObjectResult>();
    }
}


