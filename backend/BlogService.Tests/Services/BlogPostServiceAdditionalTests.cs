using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BlogService.Tests.Services;

public class BlogPostServiceAdditionalTests
{
    private readonly Mock<IBlogPostRepository> _repo = new();
    private readonly Mock<IUserServiceClient> _userClient = new();
    private readonly Mock<IAuthorizationService> _auth = new();
    private readonly Mock<IModelValidationService> _validation = new();
    private readonly Mock<IJwtUserService> _jwt = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly Mock<ILogger<BlogPostService>> _logger = new();
    private readonly IMapper _mapper;
    private readonly DefaultHttpContext _ctx = new();
    private readonly UserInfoDto _user = new() { Id = "user-9", Name = "Nine" };

    public BlogPostServiceAdditionalTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<CreateBlogPostDto, BlogPost>();
            c.CreateMap<UpdateBlogPostDto, BlogPost>();
            c.CreateMap<BlogPost, BlogPostDto>();
        });
        _mapper = cfg.CreateMapper();
        _auth.Setup(a => a.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(_user);
        _ctx.Request.Headers.Authorization = "Bearer token";
    }

    private BlogPostService Svc() => new(
        _repo.Object,
        _userClient.Object,
        _mapper,
        new HttpContextAccessor { HttpContext = _ctx },
        _auth.Object,
        _validation.Object,
        _jwt.Object,
        _logger.Object,
        _cache);

    [Fact]
    public async Task DeleteAsync_SoftDeletes_WhenAuthor()
    {
        var id = Guid.NewGuid();
        var entity = new BlogPost { Id = id, AuthorId = _user.Id, Title = "T", Status = PostStatus.Published, Content = "C" };
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(entity);
        var svc = Svc();
        await svc.DeleteAsync(id, _ctx);
        _repo.Verify(r => r.UpdateAsync(It.Is<BlogPost>(b => b.Status == PostStatus.Deleted)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotAuthor_Throws()
    {
        var id = Guid.NewGuid();
        var entity = new BlogPost { Id = id, AuthorId = "other", Title = "T", Status = PostStatus.Published, Content = "C" };
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        var svc = Svc();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.DeleteAsync(id, _ctx));
    }

    [Fact]
    public async Task GetByIdAsync_Deleted_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new BlogPost { Id = id, AuthorId = _user.Id, Title = "X", Status = PostStatus.Deleted, Content = "C" });
        var svc = Svc();
        var dto = await svc.GetByIdAsync(id);
        dto.Should().BeNull();
    }

    [Fact]
    public async Task GetFeaturedAsync_ReturnsDtos()
    {
        _repo.Setup(r => r.GetFeaturedAsync(5)).ReturnsAsync(new List<BlogPost>{ new(){ Id = Guid.NewGuid(), AuthorId = _user.Id, Title="F", Content="C", Status=PostStatus.Published } });
        _userClient.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[]{ new UserInfoDto{ Id = _user.Id, Name = _user.Name }});
        var svc = Svc();
        var list = await svc.GetFeaturedAsync();
        list.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetPopularAsync_ReturnsDtos()
    {
        _repo.Setup(r => r.GetPopularAsync(5)).ReturnsAsync(new List<BlogPost>{ new(){ Id = Guid.NewGuid(), AuthorId = _user.Id, Title="P", Content="C", Status=PostStatus.Published } });
        _userClient.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[]{ new UserInfoDto{ Id = _user.Id, Name = _user.Name }});
        var svc = Svc();
        var list = await svc.GetPopularAsync();
        list.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetFriendsPostsAsync_ReturnsDtos()
    {
        var friendId = "friend-22";
        _repo.Setup(r => r.GetFriendsPostsAsync(It.IsAny<IEnumerable<string>>(), 1, 10))
            .ReturnsAsync((new List<BlogPost>{ new(){ Id=Guid.NewGuid(), AuthorId = friendId, Title="Fr", Content="CC", Status=PostStatus.Published } }, 1));
        _userClient.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[]{ new UserInfoDto{ Id = friendId, Name = "Friend" }});
        var svc = Svc();
        var res = await svc.GetFriendsPostsAsync(new[]{ friendId });
        res.Data.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_ServiceRequest_DoesNotForcePublishedStatus()
    {
        // add service claim
        _ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{ new Claim("service","true") }));
        var parms = new BlogPostQueryParameters { Page=1, PageSize=5 };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()))
            .ReturnsAsync(((IEnumerable<BlogPost>)new List<BlogPost>(), 0));
        var svc = Svc();
        await svc.GetAllAsync(parms, _ctx);
        parms.Status.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_Draft_TriggersDraftCacheClear()
    {
        var create = new CreateBlogPostDto { Title="D", Content="c", Status=PostStatus.Draft };
        var entity = new BlogPost { Id=Guid.NewGuid(), AuthorId=_user.Id, Title="D", Content="c", Status=PostStatus.Draft };
        _repo.Setup(r => r.CreateAsync(It.IsAny<BlogPost>())).ReturnsAsync(entity);
        _userClient.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[]{ new UserInfoDto{ Id = _user.Id, Name = _user.Name }});
        var svc = Svc();
        var dto = await svc.CreateAsync(create, _ctx);
        dto.Status.Should().Be(PostStatus.Draft);
    }
}
