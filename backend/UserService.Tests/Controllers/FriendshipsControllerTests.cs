using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Controllers;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Tests.Controllers;

public class FriendshipsControllerTests
{
    private static FriendshipsController Create(out Mock<IFriendshipService> svc)
    {
        svc = new Mock<IFriendshipService>(MockBehavior.Strict);
        var ctrl = new FriendshipsController(svc.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return ctrl;
    }

    [Fact]
    public async Task All_Actions_Delegate_To_Service()
    {
        var ctrl = Create(out var svc);
        svc.Setup(s => s.GetFriendsAsync(ctrl.HttpContext)).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.GetFriends()).Should().BeOfType<OkObjectResult>();

        svc.Setup(s => s.GetPendingRequestsAsync(ctrl.HttpContext)).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.GetPendingRequests()).Should().BeOfType<OkObjectResult>();

        svc.Setup(s => s.GetAvailableUsersAsync(ctrl.HttpContext)).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.GetAvailableUsers()).Should().BeOfType<OkObjectResult>();

        var create = new CreateFriendRequestDto { ReceiverId = "u2" };
        svc.Setup(s => s.SendFriendRequestAsync(ctrl.HttpContext, create)).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.SendFriendRequest(create)).Should().BeOfType<OkObjectResult>();

        svc.Setup(s => s.AcceptFriendRequestAsync(ctrl.HttpContext, It.IsAny<Guid>())).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.AcceptFriendRequest(Guid.NewGuid())).Should().BeOfType<OkObjectResult>();

        svc.Setup(s => s.RejectFriendRequestAsync(ctrl.HttpContext, It.IsAny<Guid>())).ReturnsAsync(new OkObjectResult(new { }));
        (await ctrl.RejectFriendRequest(Guid.NewGuid())).Should().BeOfType<OkObjectResult>();

        svc.Setup(s => s.RemoveFriendAsync(ctrl.HttpContext, "u2")).ReturnsAsync(new NoContentResult());
        (await ctrl.RemoveFriend("u2")).Should().BeOfType<NoContentResult>();

        svc.Setup(s => s.CheckFriendshipAsync(ctrl.HttpContext, "u3")).ReturnsAsync(new OkObjectResult(true));
        (await ctrl.CheckFriendship("u3")).Should().BeOfType<OkObjectResult>();
    }
}


