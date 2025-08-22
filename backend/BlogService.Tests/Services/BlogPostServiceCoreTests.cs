using System.Security.Claims;
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

namespace BlogService.Tests.Services;

public class BlogPostServiceCoreTests
{
    private readonly Mock<IBlogPostRepository> _repo = new();
    private readonly Mock<IUserServiceClient> _userClient = new();
    private readonly IMapper _mapper;
    private readonly Mock<IAuthorizationService> _auth = new();
    private readonly Mock<IModelValidationService> _validation = new();
    private readonly Mock<IJwtUserService> _jwt = new();
    private readonly Mock<ILogger<BlogPostService>> _logger = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly DefaultHttpContext _httpContext = new();

    private readonly UserInfoDto _currentUser = new() { Id = "user-1", Name = "Alice", Image = "assets/profile-pictures/a.png" };

    public BlogPostServiceCoreTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<CreateBlogPostDto, BlogPost>();
            c.CreateMap<UpdateBlogPostDto, BlogPost>();
            c.CreateMap<BlogPost, BlogPostDto>();
        });
        _mapper = cfg.CreateMapper();
        _auth.Setup(a => a.GetCurrentUserAsync(It.IsAny<HttpContext>())).ReturnsAsync(() => _currentUser);
        _httpContext.Request.Headers.Authorization = "Bearer testtoken";
    }

    private BlogPostService CreateService() => new(
        _repo.Object,
        _userClient.Object,
        _mapper,
        new HttpContextAccessor { HttpContext = _httpContext },
        _auth.Object,
        _validation.Object,
        _jwt.Object,
        _logger.Object,
        _cache);

    [Fact]
    public async Task CreateAsync_Published_Post_SendsFriendNotifications_WhenFriendsExist()
    {
        // Arrange
        var service = CreateService();
        var createDto = new CreateBlogPostDto { Title = "T", Content = "C", Status = PostStatus.Published };
        var created = new BlogPost { Id = Guid.NewGuid(), Title = "T", Content = "C", Status = PostStatus.Published, AuthorId = _currentUser.Id };
        _repo.Setup(r => r.CreateAsync(It.IsAny<BlogPost>())).ReturnsAsync(created);
        _userClient.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[] { new UserInfoDto { Id = _currentUser.Id, Name = _currentUser.Name, Image = _currentUser.Image } });
        _userClient.Setup(c => c.GetUserFriendsAsync(It.IsAny<string>())).ReturnsAsync(new[] { new FriendDto { UserId = "friend-1" } });
        _userClient.Setup(c => c.CreateNotificationAsync("friend-1", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(true);

        // Act
        var dto = await service.CreateAsync(createDto, _httpContext);

        // Assert
        dto.Id.Should().Be(created.Id);
        _userClient.Verify(c => c.CreateNotificationAsync("friend-1", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonAuthor_Throws()
    {
        // Arrange
        var service = CreateService();
        var postId = Guid.NewGuid();
        var existing = new BlogPost { Id = postId, AuthorId = "other-user", Title = "Old", Status = PostStatus.Draft };
        _repo.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(existing);

        // Act
        Func<Task> act = async () => await service.UpdateAsync(postId, new UpdateBlogPostDto { Title = "New" }, _httpContext);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task PublishDraftAsync_ChangesStatus_And_NotifiesFriends()
    {
        // Arrange
        var service = CreateService();
        var postId = Guid.NewGuid();
        var draft = new BlogPost { Id = postId, AuthorId = _currentUser.Id, Title = "Draft", Status = PostStatus.Draft };
        _repo.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(draft);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<BlogPost>())).ReturnsAsync((BlogPost b) => b);
        _userClient.Setup(c => c.GetUserFriendsAsync(It.IsAny<string>())).ReturnsAsync(Enumerable.Empty<FriendDto>()); // no notifications
        _userClient.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[] { new UserInfoDto { Id = _currentUser.Id, Name = _currentUser.Name } });

        // Act
        var dto = await service.PublishDraftAsync(postId, _httpContext);

        // Assert
        dto.Status.Should().Be(PostStatus.Published);
        _repo.Verify(r => r.UpdateAsync(It.Is<BlogPost>(b => b.Status == PostStatus.Published)), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_NoStatus_FromRegularRequest_DefaultsToPublished()
    {
        // Arrange regular request (no service claim)
        var service = CreateService();
        var parms = new BlogPostQueryParameters { Page = 1, PageSize = 5 };
        var posts = new List<BlogPost> { new() { Id = Guid.NewGuid(), AuthorId = _currentUser.Id, Status = PostStatus.Published, Title = "P" } };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()))
            .ReturnsAsync(((IEnumerable<BlogPost>)posts, 1));
        _userClient.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[] { new UserInfoDto { Id = _currentUser.Id, Name = _currentUser.Name } });

        // Act
        var result = await service.GetAllAsync(parms, _httpContext);

        // Assert
        parms.Status.Should().Be(PostStatus.Published); // default applied
        result.Data.Count().Should().Be(1);
    }

    [Fact]
    public async Task ConvertToDraftAsync_FromPublished_SetsDraft()
    {
        // Arrange
        var service = CreateService();
        var postId = Guid.NewGuid();
        var published = new BlogPost { Id = postId, AuthorId = _currentUser.Id, Title = "Pub", Status = PostStatus.Published };
        _repo.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(published);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<BlogPost>())).ReturnsAsync((BlogPost b) => b);
        _userClient.Setup(c => c.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .ReturnsAsync(new[] { new UserInfoDto { Id = _currentUser.Id, Name = _currentUser.Name } });

        // Act
        var dto = await service.ConvertToDraftAsync(postId, _httpContext);

        // Assert
        dto.Status.Should().Be(PostStatus.Draft);
    }
}
