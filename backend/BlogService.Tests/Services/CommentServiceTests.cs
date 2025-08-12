using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using BlogService.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class CommentServiceTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public CommentServiceTests(MappingFixture mapping)
    {
        _mapper = mapping.Mapper;
    }

    private static CommentService CreateService(
        IMapper mapper,
        out Mock<ICommentRepository> commentRepo,
        out Mock<IBlogPostRepository> postRepo,
        out Mock<IUserServiceClient> userClient,
        out Mock<IAuthorizationService> auth,
        out Mock<IModelValidationService> validator)
    {
        commentRepo = new Mock<ICommentRepository>(MockBehavior.Strict);
        postRepo = new Mock<IBlogPostRepository>(MockBehavior.Strict);
        userClient = new Mock<IUserServiceClient>(MockBehavior.Loose);
        auth = new Mock<IAuthorizationService>(MockBehavior.Loose);
        validator = new Mock<IModelValidationService>(MockBehavior.Loose);
        var logger = new Mock<ILogger<CommentService>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new CommentService(commentRepo.Object, postRepo.Object, mapper, userClient.Object, auth.Object, validator.Object, logger.Object, cache);
    }

    [Fact]
    public async Task Create_Fails_When_BlogPost_Missing()
    {
        var service = CreateService(_mapper, out var commentRepo, out var postRepo, out var userClient, out var auth, out var validator);
        var dto = new CreateCommentDto { BlogPostId = Guid.NewGuid(), Content = "c" };
        postRepo.Setup(p => p.ExistsAsync(dto.BlogPostId)).ReturnsAsync(false);

        Func<Task> act = async () => await service.CreateAsync(dto);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Create_Fails_When_Parent_Missing()
    {
        var service = CreateService(_mapper, out var commentRepo, out var postRepo, out var userClient, out var auth, out var validator);
        var dto = new CreateCommentDto { BlogPostId = Guid.NewGuid(), ParentCommentId = Guid.NewGuid(), Content = "c" };
        postRepo.Setup(p => p.ExistsAsync(dto.BlogPostId)).ReturnsAsync(true);
        commentRepo.Setup(c => c.ExistsAsync(dto.ParentCommentId.Value)).ReturnsAsync(false);

        Func<Task> act = async () => await service.CreateAsync(dto);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Create_Succeeds_Maps_To_Dto()
    {
        var service = CreateService(_mapper, out var commentRepo, out var postRepo, out var userClient, out var auth, out var validator);
        var dto = new CreateCommentDto { BlogPostId = Guid.NewGuid(), Content = "hello", AuthorId = "u" };
        postRepo.Setup(p => p.ExistsAsync(dto.BlogPostId)).ReturnsAsync(true);
        commentRepo.Setup(c => c.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        commentRepo.Setup(c => c.CreateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => { c.Id = Guid.NewGuid(); return c; });

        userClient.Setup(u => u.GetUserByIdAsync("u")).ReturnsAsync(new UserInfoDto { Id = "u", Name = "User" });

        var result = await service.CreateAsync(dto);
        result.Id.Should().NotBe(Guid.Empty);
        result.Content.Should().Be("hello");
        result.AuthorName.Should().Be("User");
    }

    [Fact]
    public async Task Update_Only_Changes_Content_Preserving_Author()
    {
        var service = CreateService(_mapper, out var commentRepo, out var postRepo, out var userClient, out var auth, out var validator);
        var id = Guid.NewGuid();
        var existing = new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "u", AuthorName = "User", Content = "old" };
        commentRepo.Setup(c => c.GetByIdAsync(id)).ReturnsAsync(existing);
        commentRepo.Setup(c => c.UpdateAsync(existing)).ReturnsAsync(existing);
        userClient.Setup(u => u.GetUserByIdAsync("u")).ReturnsAsync(new UserInfoDto { Id = "u", Name = "User" });

        var updated = await service.UpdateAsync(id, new CreateCommentDto { Content = "new" });
        updated.Content.Should().Be("new");
        updated.AuthorName.Should().Be("User");
    }

    [Fact]
    public async Task GetByBlogPostId_Populates_Authors_Recursively()
    {
        var service = CreateService(_mapper, out var commentRepo, out var postRepo, out var userClient, out var auth, out var validator);
        var blogId = Guid.NewGuid();
        var top = new Comment { Id = Guid.NewGuid(), BlogPostId = blogId, AuthorId = "a", AuthorName = string.Empty, Content = "top", Replies = new List<Comment>() };
        var reply1 = new Comment { Id = Guid.NewGuid(), BlogPostId = blogId, AuthorId = "b", AuthorName = string.Empty, Content = "r1", Replies = new List<Comment>() };
        var reply2 = new Comment { Id = Guid.NewGuid(), BlogPostId = blogId, AuthorId = "c", AuthorName = string.Empty, Content = "r2", Replies = new List<Comment>() };
        top.Replies.Add(reply1);
        reply1.Replies.Add(reply2);

        commentRepo.Setup(c => c.GetByBlogPostIdAsync(blogId)).ReturnsAsync(new[] { top });
        userClient.Setup(u => u.GetUserByIdAsync("a")).ReturnsAsync(new UserInfoDto { Id = "a", Name = "A" });
        userClient.Setup(u => u.GetUserByIdAsync("b")).ReturnsAsync(new UserInfoDto { Id = "b", Name = "B" });
        userClient.Setup(u => u.GetUserByIdAsync("c")).ReturnsAsync(new UserInfoDto { Id = "c", Name = "C" });

        var result = await service.GetByBlogPostIdAsync(blogId);
        var list = result.ToList();
        list.Should().HaveCount(1);
        list[0].AuthorName.Should().Be("A");
        list[0].Replies[0].AuthorName.Should().Be("B");
        list[0].Replies[0].Replies[0].AuthorName.Should().Be("C");
    }

    [Fact]
    public async Task HttpContext_Create_Throws_When_No_User()
    {
        var service = CreateService(_mapper, out var commentRepo, out var postRepo, out var userClient, out var auth, out var validator);
        var http = new DefaultHttpContext();
        var dto = new CreateCommentDto { BlogPostId = Guid.NewGuid(), Content = "c" };

        validator.Setup(v => v.ValidateModel(It.IsAny<ModelStateDictionary>())).Returns(new ValidationResult { IsValid = true });
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync((UserInfoDto?)null);

        Func<Task> act = async () => await service.CreateAsync(dto, http, new ModelStateDictionary());
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetById_Returns_Null_When_NotFound()
    {
        var service = CreateService(_mapper, out var commentRepo, out var postRepo, out var userClient, out var auth, out var validator);
        var id = Guid.NewGuid();
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Comment?)null);

        var result = await service.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_Cache_Miss_Then_Hit()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out var userClient, out _, out _);
        var id = Guid.NewGuid();
        var entity = new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "u1", AuthorName = string.Empty, Content = "c" };
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        userClient.Setup(u => u.GetUserByIdAsync("u1")).ReturnsAsync(new UserInfoDto { Id = "u1", Name = "User" });

        var first = await service.GetByIdAsync(id);
        var second = await service.GetByIdAsync(id);

        first!.AuthorName.Should().Be("User");
        second!.AuthorName.Should().Be("User");
        commentRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByBlogPost_Populate_Author_Anonymous_When_AuthorId_Empty()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out var userClient, out _, out _);
        var blogId = Guid.NewGuid();
        var entity = new Comment { Id = Guid.NewGuid(), BlogPostId = blogId, AuthorId = string.Empty, AuthorName = string.Empty, Content = "c", Replies = new List<Comment>() };
        commentRepo.Setup(r => r.GetByBlogPostIdAsync(blogId)).ReturnsAsync(new[] { entity });

        var result = (await service.GetByBlogPostIdAsync(blogId)).ToList();
        result.Should().HaveCount(1);
        result[0].AuthorName.Should().Be("Anonymous");
        result[0].AuthorImage.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_UserClient_Returns_Null_Sets_Unknown()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out var userClient, out _, out _);
        var id = Guid.NewGuid();
        var entity = new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "ux", AuthorName = string.Empty, Content = "c" };
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        userClient.Setup(u => u.GetUserByIdAsync("ux")).ReturnsAsync((UserInfoDto?)null);

        var result = await service.GetByIdAsync(id);
        result!.AuthorName.Should().Be("Unknown User");
        result.AuthorImage.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_UserClient_Throws_Sets_Unknown()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out var userClient, out _, out _);
        var id = Guid.NewGuid();
        var entity = new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "ux", AuthorName = string.Empty, Content = "c" };
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        userClient.Setup(u => u.GetUserByIdAsync("ux")).ThrowsAsync(new Exception("boom"));

        var result = await service.GetByIdAsync(id);
        result!.AuthorName.Should().Be("Unknown User");
        result.AuthorImage.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_HttpContext_Success_When_Author()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out var userClient, out var auth, out var validator);
        var http = new DefaultHttpContext();
        validator.Setup(v => v.ValidateModel(It.IsAny<ModelStateDictionary>())).Returns(new ValidationResult { IsValid = true });
        var id = Guid.NewGuid();
        var user = new UserInfoDto { Id = "u1", Name = "User" };
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(user);
        var existing = new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "u1", AuthorName = "User", Content = "old" };
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        commentRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);
        userClient.Setup(u => u.GetUserByIdAsync("u1")).ReturnsAsync(user);

        var result = await service.UpdateAsync(id, new CreateCommentDto { Content = "new" }, http, new ModelStateDictionary());
        result.Content.Should().Be("new");
    }

    [Fact]
    public async Task Update_HttpContext_Invalid_Model_Throws()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out _, out var auth, out var validator);
        var http = new DefaultHttpContext();
        validator.Setup(v => v.ValidateModel(It.IsAny<ModelStateDictionary>())).Returns(new ValidationResult { IsValid = false, ErrorMessage = "err" });

        Func<Task> act = async () => await service.UpdateAsync(Guid.NewGuid(), new CreateCommentDto { Content = "x" }, http, new ModelStateDictionary());
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Update_HttpContext_Unauthorized_When_No_User()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out _, out var auth, out var validator);
        var http = new DefaultHttpContext();
        validator.Setup(v => v.ValidateModel(It.IsAny<ModelStateDictionary>())).Returns(new ValidationResult { IsValid = true });
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync((UserInfoDto?)null);

        Func<Task> act = async () => await service.UpdateAsync(Guid.NewGuid(), new CreateCommentDto { Content = "x" }, http, new ModelStateDictionary());
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Update_HttpContext_Forbid_When_Not_Author()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out _, out var auth, out var validator);
        var http = new DefaultHttpContext();
        validator.Setup(v => v.ValidateModel(It.IsAny<ModelStateDictionary>())).Returns(new ValidationResult { IsValid = true });
        var id = Guid.NewGuid();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "u2", Name = "U" });
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "u1", AuthorName = "A", Content = "c" });

        Func<Task> act = async () => await service.UpdateAsync(id, new CreateCommentDto { Content = "x" }, http, new ModelStateDictionary());
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_HttpContext_Unauthorized_When_No_User()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out _, out var auth, out _);
        var http = new DefaultHttpContext();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync((UserInfoDto?)null);

        Func<Task> act = async () => await service.DeleteAsync(Guid.NewGuid(), http);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_HttpContext_Forbid_When_Not_Author()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out _, out var auth, out _);
        var http = new DefaultHttpContext();
        var id = Guid.NewGuid();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "u2", Name = "U" });
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "u1", AuthorName = "A", Content = "c" });

        Func<Task> act = async () => await service.DeleteAsync(id, http);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_HttpContext_Soft_Delete_Returns_Updated_Dto()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out var userClient, out var auth, out _);
        var http = new DefaultHttpContext();
        var id = Guid.NewGuid();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "u1", Name = "U" });
        var existing = new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "u1", AuthorName = "U", Content = "c" };
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        commentRepo.Setup(r => r.HasRepliesAsync(id)).ReturnsAsync(true);
        commentRepo.Setup(r => r.UpdateAsync(It.IsAny<Comment>())).ReturnsAsync((Comment c) => c);
        commentRepo.Setup(r => r.GetByIdWithRepliesAsync(id)).ReturnsAsync(existing);
        userClient.Setup(u => u.GetUserByIdAsync("u1")).ReturnsAsync(new UserInfoDto { Id = "u1", Name = "U" });

        var result = await service.DeleteAsync(id, http);
        result.Should().NotBeNull();
        result!.Content.Should().Be("[deleted]");
    }

    [Fact]
    public async Task Delete_HttpContext_Hard_Delete_Returns_Null()
    {
        var service = CreateService(_mapper, out var commentRepo, out _, out _, out var auth, out _);
        var http = new DefaultHttpContext();
        var id = Guid.NewGuid();
        auth.Setup(a => a.GetCurrentUserAsync(http)).ReturnsAsync(new UserInfoDto { Id = "u1", Name = "U" });
        var existing = new Comment { Id = id, BlogPostId = Guid.NewGuid(), AuthorId = "u1", AuthorName = "U", Content = "c" };
        commentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        commentRepo.Setup(r => r.HasRepliesAsync(id)).ReturnsAsync(false);
        commentRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await service.DeleteAsync(id, http);
        result.Should().BeNull();
    }
}


