using BlogService.Controllers;
using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Tests.Controllers;

public class CommentLikesControllerTests
{
    private static CommentLikesController CreateController(out Mock<ICommentLikeService> likeService)
    {
        likeService = new Mock<ICommentLikeService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<CommentLikesController>>();
        var controller = new CommentLikesController(likeService.Object, logger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    [Fact]
    public async Task GetLikeStats_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        var commentId = Guid.NewGuid();
        var stats = new CommentLikeStatsDto { CommentId = commentId, LikeCount = 1, DislikeCount = 0 };
        svc.Setup(s => s.GetLikeStatsAsync(commentId, controller.HttpContext)).ReturnsAsync(stats);

        var action = await controller.GetLikeStats(commentId) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be(stats);
    }

    [Fact]
    public async Task CreateOrUpdateLike_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        var dto = new CreateCommentLikeDto { CommentId = Guid.NewGuid(), IsLike = true };
        var created = new CommentLikeDto { Id = Guid.NewGuid(), CommentId = dto.CommentId, IsLike = true };
        svc.Setup(s => s.CreateOrUpdateLikeAsync(dto, controller.HttpContext, controller.ModelState)).ReturnsAsync(created);

        var action = await controller.CreateOrUpdateLike(dto) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be(created);
    }

    [Fact]
    public async Task CreateOrUpdateLike_BadRequest_On_ArgumentException()
    {
        var controller = CreateController(out var svc);
        var dto = new CreateCommentLikeDto { CommentId = Guid.NewGuid(), IsLike = false };
        svc.Setup(s => s.CreateOrUpdateLikeAsync(dto, controller.HttpContext, controller.ModelState))
           .ThrowsAsync(new ArgumentException("x"));

        var action = await controller.CreateOrUpdateLike(dto);
        action.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteLike_NoContent_On_Success()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        svc.Setup(s => s.DeleteLikeAsync(id, controller.HttpContext)).Returns(Task.CompletedTask);

        var action = await controller.DeleteLike(id);
        action.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteLike_NotFound_On_ArgumentException()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        svc.Setup(s => s.DeleteLikeAsync(id, controller.HttpContext)).ThrowsAsync(new ArgumentException("x"));

        var action = await controller.DeleteLike(id);
        action.Should().BeOfType<NotFoundObjectResult>();
    }
}


