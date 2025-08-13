using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Controllers;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Tests.Controllers;

public class UsersControllerTests
{
    private static UsersController Create(out Mock<IUserService> svc)
    {
        svc = new Mock<IUserService>(MockBehavior.Strict);
        var ctrl = new UsersController(svc.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return ctrl;
    }

    [Fact]
    public async Task GetAll_With_Search_Goes_To_Search()
    {
        var ctrl = Create(out var svc);
        svc.Setup(s => s.SearchUsersAsync(ctrl.HttpContext, "al", 1, 10)).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.GetAll(1, 10, "al")).Should().BeOfType<OkObjectResult>();

        // Non-search path
        svc.Setup(s => s.GetAllAsync(ctrl.HttpContext, 2, 5)).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.GetAll(2, 5, "")).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_Update_Delete_Delegate_To_Service()
    {
        var ctrl = Create(out var svc);
        var created = new CreatedAtActionResult("GetById", null, new { id = "u1" }, new object());
        svc.Setup(s => s.CreateAsync(ctrl.HttpContext, It.IsAny<CreateUserDto>(), ctrl.ModelState)).ReturnsAsync(created);
        (await ctrl.Create(new CreateUserDto())).Should().BeOfType<CreatedAtActionResult>();

        svc.Setup(s => s.UpdateAsync(ctrl.HttpContext, "u1", It.IsAny<UpdateUserDto>(), ctrl.ModelState)).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.Update("u1", new UpdateUserDto())).Should().BeOfType<OkObjectResult>();

        svc.Setup(s => s.DeleteAsync(ctrl.HttpContext, "u1")).ReturnsAsync(new NoContentResult());
        (await ctrl.Delete("u1")).Should().BeOfType<NoContentResult>();

        // Ban/Unban delegate
        svc.Setup(s => s.BanUserAsync(ctrl.HttpContext, It.IsAny<BanUserRequest>())).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.BanUser("u1", new BanUserRequest())).Should().BeOfType<OkObjectResult>();
        svc.Setup(s => s.UnbanUserAsync(ctrl.HttpContext, It.IsAny<UnbanUserRequest>())).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.UnbanUser("u1", new UnbanUserRequest())).Should().BeOfType<OkObjectResult>();
    }
}


