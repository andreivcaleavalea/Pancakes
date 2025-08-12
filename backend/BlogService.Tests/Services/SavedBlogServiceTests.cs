using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using BlogService.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class SavedBlogServiceTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public SavedBlogServiceTests(MappingFixture mapping)
    {
        _mapper = mapping.Mapper;
    }

    private static SavedBlogService CreateService(
        IMapper mapper,
        out Mock<ISavedBlogRepository> savedRepo,
        out Mock<IBlogPostRepository> postRepo,
        out Mock<IBlogPostService> postService,
        out Mock<IAuthorizationService> auth)
    {
        savedRepo = new Mock<ISavedBlogRepository>(MockBehavior.Strict);
        postRepo = new Mock<IBlogPostRepository>(MockBehavior.Strict);
        postService = new Mock<IBlogPostService>(MockBehavior.Loose);
        auth = new Mock<IAuthorizationService>(MockBehavior.Loose);
        var logger = new Mock<ILogger<SavedBlogService>>();
        return new SavedBlogService(savedRepo.Object, postRepo.Object, mapper, postService.Object, auth.Object, logger.Object);
    }

    private static HttpContext Ctx() => new DefaultHttpContext();

    [Fact]
    public async Task SaveBlog_Success_Returns_CreatedAtAction()
    {
        var service = CreateService(_mapper, out var savedRepo, out var postRepo, out var postService, out var auth);
        var http = Ctx();
        var user = new UserInfoDto { Id = "me" };
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(user);

        var blogId = Guid.NewGuid();
        postRepo.Setup(p => p.ExistsAsync(blogId)).ReturnsAsync(true);
        savedRepo.Setup(r => r.GetSavedBlogAsync("me", blogId)).ReturnsAsync((BlogService.Models.Entities.SavedBlog?)null);
        savedRepo.Setup(r => r.SaveBlogAsync(It.IsAny<BlogService.Models.Entities.SavedBlog>()))
            .ReturnsAsync((BlogService.Models.Entities.SavedBlog sb) => sb);
        postService.Setup(ps => ps.GetByIdAsync(blogId)).ReturnsAsync(new BlogPostDto { Id = blogId });

        var modelState = new ModelStateDictionary();
        var dto = new CreateSavedBlogDto { BlogPostId = blogId };
        var result = await service.SaveBlogAsync(http, dto, modelState);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task SaveBlog_Duplicate_Returns_BadRequest()
    {
        var service = CreateService(_mapper, out var savedRepo, out var postRepo, out var postService, out var auth);
        var http = Ctx();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "me" });

        var blogId = Guid.NewGuid();
        postRepo.Setup(p => p.ExistsAsync(blogId)).ReturnsAsync(true);
        savedRepo.Setup(r => r.GetSavedBlogAsync("me", blogId)).ReturnsAsync(new BlogService.Models.Entities.SavedBlog { UserId = "me", BlogPostId = blogId });

        var modelState = new ModelStateDictionary();
        var dto = new CreateSavedBlogDto { BlogPostId = blogId };
        var result = await service.SaveBlogAsync(http, dto, modelState);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SaveBlog_BlogMissing_Returns_BadRequest()
    {
        var service = CreateService(_mapper, out var savedRepo, out var postRepo, out var postService, out var auth);
        var http = Ctx();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "me" });

        var blogId = Guid.NewGuid();
        postRepo.Setup(p => p.ExistsAsync(blogId)).ReturnsAsync(false);

        var modelState = new ModelStateDictionary();
        var dto = new CreateSavedBlogDto { BlogPostId = blogId };
        var result = await service.SaveBlogAsync(http, dto, modelState);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Unsave_Success_Returns_NoContent()
    {
        var service = CreateService(_mapper, out var savedRepo, out var postRepo, out var postService, out var auth);
        var http = Ctx();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "me" });

        var blogId = Guid.NewGuid();
        savedRepo.Setup(r => r.GetSavedBlogAsync("me", blogId)).ReturnsAsync(new BlogService.Models.Entities.SavedBlog { UserId = "me", BlogPostId = blogId });
        savedRepo.Setup(r => r.DeleteSavedBlogAsync("me", blogId)).Returns(Task.CompletedTask);

        var result = await service.UnsaveBlogAsync(http, blogId);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Unsave_NotSaved_Returns_NotFound()
    {
        var service = CreateService(_mapper, out var savedRepo, out var postRepo, out var postService, out var auth);
        var http = Ctx();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "me" });

        var blogId = Guid.NewGuid();
        savedRepo.Setup(r => r.GetSavedBlogAsync("me", blogId)).ReturnsAsync((BlogService.Models.Entities.SavedBlog?)null);

        var result = await service.UnsaveBlogAsync(http, blogId);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSavedBlogs_Unauthorized_Returns_Unauthorized()
    {
        var service = CreateService(_mapper, out var savedRepo, out var postRepo, out var postService, out var auth);
        var http = Ctx();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync((UserInfoDto?)null);

        var result = await service.GetSavedBlogsAsync(http);
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetSavedBlogs_Ok_Returns_Hydrated_Posts()
    {
        var service = CreateService(_mapper, out var savedRepo, out var postRepo, out var postService, out var auth);
        var http = Ctx();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "me" });

        var blogId = Guid.NewGuid();
        savedRepo.Setup(r => r.GetSavedBlogsByUserIdAsync("me")).ReturnsAsync(new[]
        {
            new BlogService.Models.Entities.SavedBlog { UserId = "me", BlogPostId = blogId }
        });
        postService.Setup(ps => ps.GetByIdAsync(blogId)).ReturnsAsync(new BlogPostDto { Id = blogId, Title = "T" });

        var result = await service.GetSavedBlogsAsync(http) as OkObjectResult;
        result.Should().NotBeNull();
        var payload = (IEnumerable<SavedBlogDto>)result!.Value!;
        payload.First().BlogPost!.Id.Should().Be(blogId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsBookmarked_Returns_Result_When_Authorized(bool bookmarked)
    {
        var service = CreateService(_mapper, out var savedRepo, out var postRepo, out var postService, out var auth);
        var http = Ctx();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "me" });

        var blogId = Guid.NewGuid();
        savedRepo.Setup(r => r.IsBookmarkedAsync("me", blogId)).ReturnsAsync(bookmarked);

        var result = await service.IsBookmarkedAsync(http, blogId) as OkObjectResult;
        result.Should().NotBeNull();
        var value = result!.Value!;
        var isBookmarkedProp = value.GetType().GetProperty("isBookmarked", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        isBookmarkedProp.Should().NotBeNull();
        var flag = (bool)isBookmarkedProp!.GetValue(value)!;
        flag.Should().Be(bookmarked);
    }
}


