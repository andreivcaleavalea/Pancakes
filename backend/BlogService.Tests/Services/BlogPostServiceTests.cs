using System.Security.Claims;
using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using BlogService.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class BlogPostServiceTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public BlogPostServiceTests(MappingFixture mapping)
    {
        _mapper = mapping.Mapper;
    }

    [Fact]
    public async Task GetById_Found_Enriches_And_Caches()
    {
        var httpContext = CreateHttpContext(bearerToken: "tkn-x");
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out _);

        var id = Guid.NewGuid();
        var entity = new BlogPost { Id = id, AuthorId = "user-1", Title = "T" };
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

        userClientMock
            .Setup(c => c.GetUserByIdAsync("user-1", "tkn-x"))
            .ReturnsAsync(new UserInfoDto { Id = "user-1", Name = "User One" });

        var first = await service.GetByIdAsync(id);
        first!.AuthorName.Should().Be("User One");

        var second = await service.GetByIdAsync(id);
        second!.AuthorName.Should().Be("User One");

        repoMock.Verify(r => r.GetByIdAsync(id), Times.Once); // cached on second call
        userClientMock.Verify(c => c.GetUserByIdAsync("user-1", "tkn-x"), Times.Once);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        var httpContext = CreateHttpContext();
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out _);

        var id = Guid.NewGuid();
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((BlogPost?)null);

        var result = await service.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_Caches_And_BatchAuthorEnrichment_Uses_Token()
    {
        var httpContext = CreateHttpContext(bearerToken: "tok-abc");
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out _);

        var list = new List<BlogPost>
        {
            new() { Id = Guid.NewGuid(), AuthorId = "A", Title = "a1" },
            new() { Id = Guid.NewGuid(), AuthorId = "A", Title = "a2" },
            new() { Id = Guid.NewGuid(), AuthorId = "B", Title = "b1" },
        };

        repoMock.Setup(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()))
            .ReturnsAsync((list, list.Count));

        userClientMock
            .Setup(c => c.GetUsersByIdsAsync(It.Is<IEnumerable<string>>(ids => ids.Distinct().Count() == 2 && ids.Contains("A") && ids.Contains("B")), "tok-abc"))
            .ReturnsAsync(new[]
            {
                new UserInfoDto { Id = "A", Name = "Alice" },
                new UserInfoDto { Id = "B", Name = "Bob" }
            });

        var p = new BlogPostQueryParameters { Page = 1, PageSize = 10 };
        var res1 = await service.GetAllAsync(p);
        var res2 = await service.GetAllAsync(p); // cached

        repoMock.Verify(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()), Times.Once);
        userClientMock.Verify(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), "tok-abc"), Times.Once);
        res1.Data.Should().HaveCount(3);
        res2.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_Empty_Does_Not_Call_UserService()
    {
        var httpContext = CreateHttpContext(bearerToken: "tok");
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out _);

        repoMock.Setup(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()))
            .ReturnsAsync((Enumerable.Empty<BlogPost>(), 0));

        var result = await service.GetAllAsync(new BlogPostQueryParameters());
        userClientMock.Verify(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task GetAll_WithHttpContext_ServiceRequest_Does_Not_Override_Status()
    {
        var httpContext = CreateHttpContext(isServiceRequest: true);
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out _);

        BlogPostQueryParameters? captured = null;
        repoMock.Setup(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()))
            .Callback<BlogPostQueryParameters>(p => captured = p)
            .ReturnsAsync((Enumerable.Empty<BlogPost>(), 0));

        var p = new BlogPostQueryParameters { Status = null };
        await service.GetAllAsync(p, httpContext);

        captured.Should().NotBeNull();
        captured!.Status.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_WithHttpContext_NonService_With_Explicit_Status_Not_Overridden()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false);
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out _);

        BlogPostQueryParameters? captured = null;
        repoMock.Setup(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()))
            .Callback<BlogPostQueryParameters>(p => captured = p)
            .ReturnsAsync((Enumerable.Empty<BlogPost>(), 0));

        var p = new BlogPostQueryParameters { Status = PostStatus.Draft };
        await service.GetAllAsync(p, httpContext);

        captured.Should().NotBeNull();
        captured!.Status.Should().Be(PostStatus.Draft);
    }

    [Fact]
    public async Task Featured_And_Popular_Cached()
    {
        var httpContext = CreateHttpContext();
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out _);

        var posts = new List<BlogPost> { new() { Id = Guid.NewGuid(), AuthorId = "A", Title = "t" } };
        repoMock.Setup(r => r.GetFeaturedAsync(2)).ReturnsAsync(posts);
        repoMock.Setup(r => r.GetPopularAsync(3)).ReturnsAsync(posts);
        userClientMock.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[] { new UserInfoDto { Id = "A", Name = "A" } });

        var f1 = await service.GetFeaturedAsync(2);
        var f2 = await service.GetFeaturedAsync(2);
        var p1 = await service.GetPopularAsync(3);
        var p2 = await service.GetPopularAsync(3);

        // Featured and Popular are cached
        repoMock.Verify(r => r.GetFeaturedAsync(2), Times.Once);
        repoMock.Verify(r => r.GetPopularAsync(3), Times.Once);
        f1.Count().Should().Be(1);
        p1.Count().Should().Be(1);
    }

    [Fact]
    public async Task Update_Service_NotFound_Throws()
    {
        var httpContext = CreateHttpContext(isServiceRequest: true);
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out _);

        var id = Guid.NewGuid();
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((BlogPost?)null);

        Func<Task> act = async () => await service.UpdateAsync(id, new UpdateBlogPostDto { Title = "T" }, httpContext);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Update_User_NotFound_Throws()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false);
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out var authMock);

        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync(new UserInfoDto { Id = "u", Name = "U" });
        var id = Guid.NewGuid();
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((BlogPost?)null);

        Func<Task> act = async () => await service.UpdateAsync(id, new UpdateBlogPostDto { Title = "T" }, httpContext);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Delete_NonService_NoUser_Throws_Unauthorized()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false);
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out var authMock);

        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync((UserInfoDto?)null);

        Func<Task> act = async () => await service.DeleteAsync(Guid.NewGuid(), httpContext);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<BlogPost>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NonService_NotAuthor_Throws_Unauthorized()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false);
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out var authMock);

        var currentUser = new UserInfoDto { Id = "u1", Name = "U" };
        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync(currentUser);

        var id = Guid.NewGuid();
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new BlogPost { Id = id, AuthorId = "other" });

        Func<Task> act = async () => await service.DeleteAsync(id, httpContext);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<BlogPost>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NonService_Author_SoftDeletes()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false);
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out var authMock);

        var currentUser = new UserInfoDto { Id = "u1", Name = "U" };
        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync(currentUser);

        var id = Guid.NewGuid();
        var existing = new BlogPost { Id = id, AuthorId = currentUser.Id, Status = PostStatus.Published };
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

        BlogPost? updated = null;
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<BlogPost>()))
            .Callback<BlogPost>(bp => updated = bp)
            .ReturnsAsync((BlogPost bp) => bp);

        await service.DeleteAsync(id, httpContext);

        updated.Should().NotBeNull();
        updated!.Status.Should().Be(PostStatus.Deleted);
    }

    [Fact]
    public async Task Create_Populates_AuthorImage_From_Relative_Path()
    {
        var httpContext = CreateHttpContext();
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out var authMock);

        var currentUser = new UserInfoDto { Id = "user-123", Name = "User" };
        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync(currentUser);

        repoMock.Setup(r => r.CreateAsync(It.IsAny<BlogPost>()))
            .ReturnsAsync((BlogPost bp) => { bp.Id = Guid.NewGuid(); return bp; });

        userClientMock.Setup(c => c.GetUserByIdAsync(currentUser.Id))
            .ReturnsAsync(new UserInfoDto { Id = currentUser.Id, Name = currentUser.Name, Image = "assets/profile-pictures/photo.png" });

        var dto = new CreateBlogPostDto { Title = "T", Content = "C", AuthorId = "ignored" };
        var result = await service.CreateAsync(dto, httpContext);

        result.AuthorName.Should().Be("User");
        result.AuthorImage.Should().Be("http://localhost:5141/assets/profile-pictures/photo.png");
    }
    private static HttpContext CreateHttpContext(bool isServiceRequest = false, string? bearerToken = null)
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>();
        if (isServiceRequest)
        {
            claims.Add(new Claim("service", "true"));
        }
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        if (!string.IsNullOrEmpty(bearerToken))
        {
            context.Request.Headers.Authorization = $"Bearer {bearerToken}";
        }
        return context;
    }

    private static BlogPostService CreateService(
        IMapper mapper,
        HttpContext httpContext,
        out Mock<IBlogPostRepository> repoMock,
        out Mock<IUserServiceClient> userClientMock,
        out Mock<IHttpContextAccessor> httpAccessorMock,
        out Mock<IAuthorizationService> authMock)
    {
        repoMock = new Mock<IBlogPostRepository>(MockBehavior.Strict);
        userClientMock = new Mock<IUserServiceClient>(MockBehavior.Loose);
        httpAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
        httpAccessorMock.SetupGet(a => a.HttpContext).Returns(httpContext);
        authMock = new Mock<IAuthorizationService>(MockBehavior.Loose);

        var modelValidationMock = new Mock<IModelValidationService>(MockBehavior.Loose);
        var jwtUserServiceMock = new Mock<IJwtUserService>(MockBehavior.Loose);
        var loggerMock = new Mock<ILogger<BlogPostService>>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        return new BlogPostService(
            repoMock.Object,
            userClientMock.Object,
            mapper,
            httpAccessorMock.Object,
            authMock.Object,
            modelValidationMock.Object,
            jwtUserServiceMock.Object,
            loggerMock.Object,
            memoryCache);
    }

    [Fact]
    public async Task GetAll_HttpContextAware_NonService_Forces_Status_Published_When_Not_Specified()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false);
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out var httpAccessorMock, out var authMock);

        repoMock
            .Setup(r => r.GetAllAsync(It.Is<BlogPostQueryParameters>(p => p.Status == PostStatus.Published)))
            .ReturnsAsync((Enumerable.Empty<BlogPost>(), 0));

        var parameters = new BlogPostQueryParameters { Status = null, Page = 1, PageSize = 10 };

        var result = await service.GetAllAsync(parameters, httpContext);

        result.Should().NotBeNull();
        result.Pagination.TotalItems.Should().Be(0);
        repoMock.Verify(r => r.GetAllAsync(It.Is<BlogPostQueryParameters>(p => p.Status == PostStatus.Published)), Times.Once);
    }

    [Fact]
    public async Task Update_ServiceRequest_Bypasses_User_Check_And_Updates()
    {
        var httpContext = CreateHttpContext(isServiceRequest: true, bearerToken: "svc-token");
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out var httpAccessorMock, out var authMock);

        var existingId = Guid.NewGuid();
        var existing = new BlogPost { Id = existingId, AuthorId = "author-1", Title = "Old" };
        repoMock.Setup(r => r.GetByIdAsync(existingId)).ReturnsAsync(existing);

        BlogPost? updatedPassed = null;
        repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<BlogPost>()))
            .Callback<BlogPost>(bp => updatedPassed = bp)
            .ReturnsAsync((BlogPost bp) => bp);

        // Avoid external user lookups during mapping of result
        userClientMock.Setup(c => c.GetUserByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new UserInfoDto { Id = existing.AuthorId, Name = "Name" });

        var dto = new UpdateBlogPostDto { Title = "New" };
        var result = await service.UpdateAsync(existingId, dto, httpContext);

        updatedPassed.Should().NotBeNull();
        updatedPassed!.Title.Should().Be("New");
        result.Title.Should().Be("New");
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<BlogPost>()), Times.Once);
        authMock.Verify(a => a.GetCurrentUserAsync(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task Update_User_Throws_Unauthorized_When_No_User()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false);
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out var authMock);

        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync((UserInfoDto?)null);

        Func<Task> act = async () => await service.UpdateAsync(Guid.NewGuid(), new UpdateBlogPostDto { Title = "T" }, httpContext);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        repoMock.Verify(r => r.UpdateAsync(It.IsAny<BlogPost>()), Times.Never);
    }

    [Fact]
    public async Task Update_User_Throws_When_Not_Author()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false);
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out var authMock);

        var currentUser = new UserInfoDto { Id = "user-1", Name = "X" };
        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync(currentUser);

        var id = Guid.NewGuid();
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new BlogPost { Id = id, AuthorId = "someone-else", Title = "Old" });

        Func<Task> act = async () => await service.UpdateAsync(id, new UpdateBlogPostDto { Title = "New" }, httpContext);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        repoMock.Verify(r => r.UpdateAsync(It.IsAny<BlogPost>()), Times.Never);
    }

    [Fact]
    public async Task Update_User_Succeeds_When_Author()
    {
        var httpContext = CreateHttpContext(isServiceRequest: false, bearerToken: "token-123");
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out var authMock);

        var currentUser = new UserInfoDto { Id = "user-1", Name = "User" };
        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync(currentUser);

        var id = Guid.NewGuid();
        var existing = new BlogPost { Id = id, AuthorId = currentUser.Id, Title = "Old" };
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

        BlogPost? captured = null;
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<BlogPost>()))
            .Callback<BlogPost>(bp => captured = bp)
            .ReturnsAsync((BlogPost bp) => bp);

        userClientMock.Setup(c => c.GetUserByIdAsync(existing.AuthorId, "token-123"))
            .ReturnsAsync(new UserInfoDto { Id = existing.AuthorId, Name = "User" });

        var result = await service.UpdateAsync(id, new UpdateBlogPostDto { Title = "New" }, httpContext);

        captured.Should().NotBeNull();
        captured!.Title.Should().Be("New");
        result.Title.Should().Be("New");
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<BlogPost>()), Times.Once);
    }

    [Fact]
    public async Task Delete_SoftDelete_Sets_Status_Deleted_And_Updates()
    {
        var httpContext = CreateHttpContext(isServiceRequest: true);
        var service = CreateService(_mapper, httpContext, out var repoMock, out _, out _, out _);

        var id = Guid.NewGuid();
        var existing = new BlogPost { Id = id, AuthorId = "author", Status = PostStatus.Published };
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

        BlogPost? updated = null;
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<BlogPost>()))
            .Callback<BlogPost>(bp => updated = bp)
            .ReturnsAsync((BlogPost bp) => bp);

        await service.DeleteAsync(id, httpContext);

        updated.Should().NotBeNull();
        updated!.Status.Should().Be(PostStatus.Deleted);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<BlogPost>()), Times.Once);
    }

    [Fact]
    public async Task Create_Sets_AuthorId_From_Current_User()
    {
        var httpContext = CreateHttpContext();
        var service = CreateService(_mapper, httpContext, out var repoMock, out var userClientMock, out _, out var authMock);

        var currentUser = new UserInfoDto { Id = "user-123", Name = "User" };
        authMock.Setup(a => a.GetCurrentUserAsync(httpContext)).ReturnsAsync(currentUser);

        BlogPost? createdPassed = null;
        repoMock.Setup(r => r.CreateAsync(It.IsAny<BlogPost>()))
            .Callback<BlogPost>(bp => createdPassed = bp)
            .ReturnsAsync((BlogPost bp) =>
            {
                // Simulate DB setting Id
                bp.Id = Guid.NewGuid();
                return bp;
            });

        userClientMock.Setup(c => c.GetUserByIdAsync(currentUser.Id))
            .ReturnsAsync(new UserInfoDto { Id = currentUser.Id, Name = currentUser.Name });

        var dto = new CreateBlogPostDto { Title = "T", Content = "C", AuthorId = "will-be-overridden" };
        var result = await service.CreateAsync(dto, httpContext);

        createdPassed.Should().NotBeNull();
        createdPassed!.AuthorId.Should().Be(currentUser.Id);
        result.AuthorId.Should().Be(currentUser.Id);
        repoMock.Verify(r => r.CreateAsync(It.IsAny<BlogPost>()), Times.Once);
    }
}


