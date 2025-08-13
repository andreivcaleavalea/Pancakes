using BlogService.Controllers;
using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Tests.Controllers;

public class CommentsControllerTests
{
    private static CommentsController CreateController(out Mock<ICommentService> svc)
    {
        svc = new Mock<ICommentService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<CommentsController>>();
        var controller = new CommentsController(svc.Object, logger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    [Fact]
    public async Task GetCommentsByBlogPost_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        var blogId = Guid.NewGuid();
        svc.Setup(s => s.GetByBlogPostIdAsync(blogId)).ReturnsAsync(new List<CommentDto>());

        var action = await controller.GetCommentsByBlogPostId(blogId) as OkObjectResult;
        action.Should().NotBeNull();
        (action!.Value as IEnumerable<CommentDto>)!.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCommentsByBlogPost_Returns_500_On_Exception()
    {
        var controller = CreateController(out var svc);
        var blogId = Guid.NewGuid();
        svc.Setup(s => s.GetByBlogPostIdAsync(blogId)).ThrowsAsync(new Exception("boom"));

        var action = await controller.GetCommentsByBlogPostId(blogId);
        action.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetComment_NotFound_Returns_404()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        svc.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((CommentDto?)null);

        var action = await controller.GetComment(id);
        action.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetComment_Found_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        var dto = new CommentDto { Id = id, Content = "C" };
        svc.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);

        var action = await controller.GetComment(id) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be(dto);
    }

    [Fact]
    public async Task CreateComment_Success_Returns_CreatedAt()
    {
        var controller = CreateController(out var svc);
        var create = new CreateCommentDto { BlogPostId = Guid.NewGuid(), Content = "c" };
        var created = new CommentDto { Id = Guid.NewGuid(), Content = "c" };
        svc.Setup(s => s.CreateAsync(create, controller.HttpContext, controller.ModelState)).ReturnsAsync(created);

        var action = await controller.CreateComment(create) as CreatedAtActionResult;
        action.Should().NotBeNull();
        action!.ActionName.Should().Be(nameof(CommentsController.GetComment));
        action.Value.Should().Be(created);
    }

    [Fact]
    public async Task CreateComment_BadRequest_On_ArgumentException()
    {
        var controller = CreateController(out var svc);
        var create = new CreateCommentDto { BlogPostId = Guid.NewGuid(), Content = "c" };
        svc.Setup(s => s.CreateAsync(create, controller.HttpContext, controller.ModelState)).ThrowsAsync(new ArgumentException("x"));

        var action = await controller.CreateComment(create);
        action.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateComment_Unauthorized_On_UnauthorizedAccess()
    {
        var controller = CreateController(out var svc);
        var create = new CreateCommentDto { BlogPostId = Guid.NewGuid(), Content = "c" };
        svc.Setup(s => s.CreateAsync(create, controller.HttpContext, controller.ModelState)).ThrowsAsync(new UnauthorizedAccessException());

        var action = await controller.CreateComment(create);
        action.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateComment_Success_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        var update = new CreateCommentDto { Content = "new" };
        var updated = new CommentDto { Id = id, Content = "new" };
        svc.Setup(s => s.UpdateAsync(id, update, controller.HttpContext, controller.ModelState)).ReturnsAsync(updated);

        var action = await controller.UpdateComment(id, update) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be(updated);
    }

    [Fact]
    public async Task UpdateComment_NotFound_On_ArgumentException()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        var update = new CreateCommentDto { Content = "new" };
        svc.Setup(s => s.UpdateAsync(id, update, controller.HttpContext, controller.ModelState)).ThrowsAsync(new ArgumentException("x"));

        var action = await controller.UpdateComment(id, update);
        action.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateComment_Unauthorized_On_UnauthorizedAccess()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        var update = new CreateCommentDto { Content = "new" };
        svc.Setup(s => s.UpdateAsync(id, update, controller.HttpContext, controller.ModelState)).ThrowsAsync(new UnauthorizedAccessException());

        var action = await controller.UpdateComment(id, update);
        action.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task DeleteComment_NoContent_On_Success()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        svc.Setup(s => s.DeleteAsync(id, controller.HttpContext)).ReturnsAsync((CommentDto?)null);

        var action = await controller.DeleteComment(id);
        action.Should().BeOfType<NoContentResult>();
    }
}


