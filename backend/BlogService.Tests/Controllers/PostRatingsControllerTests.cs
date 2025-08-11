using BlogService.Controllers;
using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Tests.Controllers;

public class PostRatingsControllerTests
{
    private static PostRatingsController CreateController(out Mock<IPostRatingService> svc)
    {
        svc = new Mock<IPostRatingService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<PostRatingsController>>();
        var controller = new PostRatingsController(svc.Object, logger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    [Fact]
    public async Task GetRatingStats_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        var stats = new PostRatingStatsDto { BlogPostId = id, AverageRating = 4.0m, TotalRatings = 5 };
        svc.Setup(s => s.GetRatingStatsAsync(id, controller.HttpContext)).ReturnsAsync(stats);

        var action = await controller.GetRatingStats(id) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be(stats);
    }

    [Fact]
    public async Task CreateOrUpdate_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        var create = new CreatePostRatingDto { BlogPostId = Guid.NewGuid(), Rating = 3.5m };
        var dto = new PostRatingDto { Id = Guid.NewGuid(), BlogPostId = create.BlogPostId, Rating = 3.5m };
        svc.Setup(s => s.CreateOrUpdateRatingAsync(create, controller.HttpContext, controller.ModelState)).ReturnsAsync(dto);

        var action = await controller.CreateOrUpdateRating(create) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be(dto);
    }

    [Fact]
    public async Task CreateOrUpdate_BadRequest_On_ArgumentException()
    {
        var controller = CreateController(out var svc);
        var create = new CreatePostRatingDto { BlogPostId = Guid.NewGuid(), Rating = 0.1m };
        svc.Setup(s => s.CreateOrUpdateRatingAsync(create, controller.HttpContext, controller.ModelState))
           .ThrowsAsync(new ArgumentException("x"));

        var action = await controller.CreateOrUpdateRating(create);
        action.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteRating_NoContent_On_Success()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        svc.Setup(s => s.DeleteRatingAsync(id, controller.HttpContext)).Returns(Task.CompletedTask);

        var action = await controller.DeleteRating(id);
        action.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteRating_NotFound_On_ArgumentException()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        svc.Setup(s => s.DeleteRatingAsync(id, controller.HttpContext)).ThrowsAsync(new ArgumentException("x"));

        var action = await controller.DeleteRating(id);
        action.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteRating_Unauthorized_On_UnauthorizedAccess()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        svc.Setup(s => s.DeleteRatingAsync(id, controller.HttpContext)).ThrowsAsync(new UnauthorizedAccessException());

        var action = await controller.DeleteRating(id);
        action.Should().BeOfType<UnauthorizedObjectResult>();
    }
}



