using BlogService.Controllers;
using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Tests.Controllers;

public class SavedBlogsControllerTests
{
    private static SavedBlogsController CreateController(out Mock<ISavedBlogService> svc)
    {
        svc = new Mock<ISavedBlogService>(MockBehavior.Strict);
        var controller = new SavedBlogsController(svc.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    [Fact]
    public async Task GetSavedBlogs_Delegates_To_Service()
    {
        var controller = CreateController(out var svc);
        var ok = new OkObjectResult(new List<SavedBlogDto>());
        svc.Setup(s => s.GetSavedBlogsAsync(controller.HttpContext)).ReturnsAsync(ok);

        var action = await controller.GetSavedBlogs();
        action.Should().Be(ok);
    }

    [Fact]
    public async Task SaveBlog_Delegates_To_Service()
    {
        var controller = CreateController(out var svc);
        var payload = new CreateSavedBlogDto { BlogPostId = Guid.NewGuid() };
        var created = new CreatedAtActionResult("GetSavedBlogs", null, null, new SavedBlogDto());
        svc.Setup(s => s.SaveBlogAsync(controller.HttpContext, payload, controller.ModelState)).ReturnsAsync(created);

        var action = await controller.SaveBlog(payload);
        action.Should().Be(created);
    }

    [Fact]
    public async Task UnsaveBlog_Delegates_To_Service()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        var noContent = new NoContentResult();
        svc.Setup(s => s.UnsaveBlogAsync(controller.HttpContext, id)).ReturnsAsync(noContent);

        var action = await controller.UnsaveBlog(id);
        action.Should().Be(noContent);
    }

    [Fact]
    public async Task IsBookmarked_Delegates_To_Service()
    {
        var controller = CreateController(out var svc);
        var id = Guid.NewGuid();
        var ok = new OkObjectResult(new { isBookmarked = true });
        svc.Setup(s => s.IsBookmarkedAsync(controller.HttpContext, id)).ReturnsAsync(ok);

        var action = await controller.IsBookmarked(id);
        action.Should().Be(ok);
    }
}



