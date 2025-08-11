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
}


