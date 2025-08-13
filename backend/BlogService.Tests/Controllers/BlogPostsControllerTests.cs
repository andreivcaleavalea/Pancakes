using BlogService.Controllers;
using BlogService.Models.DTOs;
using BlogService.Models.Requests;
using BlogService.Services.Interfaces;
using BlogService.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Tests.Controllers;

public class BlogPostsControllerTests
{
    private static BlogPostsController CreateController(
        out Mock<IBlogPostService> blogService,
        out Mock<IFriendsPostService> friendsService)
    {
        blogService = new Mock<IBlogPostService>(MockBehavior.Strict);
        friendsService = new Mock<IFriendsPostService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<BlogPostsController>>();
        var controller = new BlogPostsController(blogService.Object, logger.Object, friendsService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    [Fact]
    public async Task GetBlogPosts_Returns_Ok_With_Paginated_Result()
    {
        var controller = CreateController(out var blogService, out var friendsService);
        var parameters = new BlogPostQueryParameters { Page = 1, PageSize = 10 };
        var resultPayload = PaginationHelper.CreatePaginatedResult(new List<BlogPostDto>(), 1, 10, 0);
        blogService
            .Setup(s => s.GetAllAsync(parameters, controller.HttpContext))
            .ReturnsAsync(resultPayload);

        var action = await controller.GetBlogPosts(parameters) as OkObjectResult;

        action.Should().NotBeNull();
        action!.Value.Should().BeEquivalentTo(resultPayload);
    }

    [Fact]
    public async Task GetBlogPosts_Returns_500_On_Exception()
    {
        var controller = CreateController(out var blogService, out _);
        var parameters = new BlogPostQueryParameters { Page = 1, PageSize = 10 };
        blogService
            .Setup(s => s.GetAllAsync(parameters, controller.HttpContext))
            .ThrowsAsync(new Exception("boom"));

        var action = await controller.GetBlogPosts(parameters);
        action.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetBlogPost_NotFound_Returns_404()
    {
        var controller = CreateController(out var blogService, out _);
        var id = Guid.NewGuid();
        blogService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((BlogPostDto?)null);

        var action = await controller.GetBlogPost(id);
        action.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetBlogPost_Found_Returns_Ok()
    {
        var controller = CreateController(out var blogService, out _);
        var id = Guid.NewGuid();
        var dto = new BlogPostDto { Id = id, Title = "T" };
        blogService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);

        var action = await controller.GetBlogPost(id) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be(dto);
    }

    [Fact]
    public async Task CreateBlogPost_Success_Returns_CreatedAt()
    {
        var controller = CreateController(out var blogService, out _);
        var dto = new CreateBlogPostDto { Title = "T", Content = "C", AuthorId = "u" };
        var created = new BlogPostDto { Id = Guid.NewGuid(), Title = "T" };
        blogService.Setup(s => s.CreateAsync(dto, controller.HttpContext)).ReturnsAsync(created);

        var action = await controller.CreateBlogPost(dto) as CreatedAtActionResult;
        action.Should().NotBeNull();
        action!.ActionName.Should().Be(nameof(BlogPostsController.GetBlogPost));
        action.Value.Should().Be(created);
    }

    [Fact]
    public async Task CreateBlogPost_BadRequest_On_ArgumentException()
    {
        var controller = CreateController(out var blogService, out _);
        var dto = new CreateBlogPostDto { Title = "T", Content = "C", AuthorId = "u" };
        blogService.Setup(s => s.CreateAsync(dto, controller.HttpContext)).ThrowsAsync(new ArgumentException("x"));

        var action = await controller.CreateBlogPost(dto);
        action.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateBlogPost_Forbidden_On_UnauthorizedAccess()
    {
        var controller = CreateController(out var blogService, out _);
        var id = Guid.NewGuid();
        var dto = new UpdateBlogPostDto { Title = "T" };
        blogService.Setup(s => s.UpdateAsync(id, dto, controller.HttpContext)).ThrowsAsync(new UnauthorizedAccessException());

        var action = await controller.UpdateBlogPost(id, dto);
        action.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task UpdateBlogPost_NotFound_On_ArgumentException()
    {
        var controller = CreateController(out var blogService, out _);
        var id = Guid.NewGuid();
        var dto = new UpdateBlogPostDto { Title = "T" };
        blogService.Setup(s => s.UpdateAsync(id, dto, controller.HttpContext)).ThrowsAsync(new ArgumentException("x"));

        var action = await controller.UpdateBlogPost(id, dto);
        action.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateBlogPost_Success_Returns_Ok()
    {
        var controller = CreateController(out var blogService, out _);
        var id = Guid.NewGuid();
        var dto = new UpdateBlogPostDto { Title = "T" };
        var updated = new BlogPostDto { Id = id, Title = "T" };
        blogService.Setup(s => s.UpdateAsync(id, dto, controller.HttpContext)).ReturnsAsync(updated);

        var action = await controller.UpdateBlogPost(id, dto) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().Be(updated);
    }

    [Fact]
    public async Task DeleteBlogPost_Forbidden_On_UnauthorizedAccess()
    {
        var controller = CreateController(out var blogService, out _);
        var id = Guid.NewGuid();
        blogService.Setup(s => s.DeleteAsync(id, controller.HttpContext)).ThrowsAsync(new UnauthorizedAccessException());

        var action = await controller.DeleteBlogPost(id);
        action.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteBlogPost_NotFound_On_ArgumentException()
    {
        var controller = CreateController(out var blogService, out _);
        var id = Guid.NewGuid();
        blogService.Setup(s => s.DeleteAsync(id, controller.HttpContext)).ThrowsAsync(new ArgumentException("x"));

        var action = await controller.DeleteBlogPost(id);
        action.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteBlogPost_Success_Returns_NoContent()
    {
        var controller = CreateController(out var blogService, out _);
        var id = Guid.NewGuid();
        blogService.Setup(s => s.DeleteAsync(id, controller.HttpContext)).Returns(Task.CompletedTask);

        var action = await controller.DeleteBlogPost(id);
        action.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetFeatured_Returns_Ok()
    {
        var controller = CreateController(out var blogService, out _);
        blogService.Setup(s => s.GetFeaturedAsync(5)).ReturnsAsync(new List<BlogPostDto> { new() { Id = Guid.NewGuid() } });

        var action = await controller.GetFeaturedPosts(5) as OkObjectResult;
        action.Should().NotBeNull();
        (action!.Value as IEnumerable<BlogPostDto>)!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPopular_Returns_Ok()
    {
        var controller = CreateController(out var blogService, out _);
        blogService.Setup(s => s.GetPopularAsync(3)).ReturnsAsync(new List<BlogPostDto>());

        var action = await controller.GetPopularPosts(3) as OkObjectResult;
        action.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFriendsPosts_Unauthorized_Returns_Unauthorized()
    {
        var controller = CreateController(out _, out var friendsService);
        friendsService.Setup(f => f.GetFriendsPostsAsync(controller.HttpContext, 1, 10)).ThrowsAsync(new UnauthorizedAccessException());

        var action = await controller.GetFriendsPosts(1, 10);
        action.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetFriendsPosts_Success_Returns_Ok()
    {
        var controller = CreateController(out _, out var friendsService);
        var payload = PaginationHelper.CreatePaginatedResult(new List<BlogPostDto>(), 1, 10, 0);
        friendsService.Setup(f => f.GetFriendsPostsAsync(controller.HttpContext, 1, 10)).ReturnsAsync(payload);

        var action = await controller.GetFriendsPosts(1, 10) as OkObjectResult;
        action.Should().NotBeNull();
        action!.Value.Should().BeEquivalentTo(payload);
    }
}


