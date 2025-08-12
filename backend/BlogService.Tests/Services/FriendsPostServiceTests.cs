using BlogService.Models.DTOs;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using BlogService.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class FriendsPostServiceTests
{
    private static FriendsPostService CreateService(
        out Mock<IUserServiceClient> userClient,
        out Mock<IBlogPostService> blogPostService,
        out Mock<IJwtUserService> jwtUserService)
    {
        userClient = new Mock<IUserServiceClient>(MockBehavior.Strict);
        blogPostService = new Mock<IBlogPostService>(MockBehavior.Strict);
        jwtUserService = new Mock<IJwtUserService>(MockBehavior.Strict);
        var logger = new Mock<ILogger<FriendsPostService>>();
        return new FriendsPostService(userClient.Object, blogPostService.Object, jwtUserService.Object, logger.Object);
    }

    private static HttpContext CreateHttpContext(string? bearer = null)
    {
        var ctx = new DefaultHttpContext();
        if (!string.IsNullOrEmpty(bearer))
        {
            ctx.Request.Headers.Authorization = $"Bearer {bearer}";
        }
        return ctx;
    }

    [Fact]
    public async Task Unauthorized_When_No_Current_User()
    {
        var service = CreateService(out var userClient, out var blogPostService, out var jwt);
        jwt.Setup(j => j.GetCurrentUserId()).Returns((string?)null);

        var ctx = CreateHttpContext("token");
        Func<Task> act = async () => await service.GetFriendsPostsAsync(ctx, 1, 10);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task No_Friends_Returns_Empty_Result()
    {
        var service = CreateService(out var userClient, out var blogPostService, out var jwt);
        jwt.Setup(j => j.GetCurrentUserId()).Returns("me");
        userClient.Setup(u => u.GetUserFriendsAsync("token")).ReturnsAsync(Array.Empty<FriendDto>());

        var ctx = CreateHttpContext("token");
        var result = await service.GetFriendsPostsAsync(ctx, 2, 5);

        // result is an anonymous type from another assembly; validate via reflection
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        var type = result.GetType();
        var paginationProp = type.GetProperty("pagination", flags);
        var dataProp = type.GetProperty("data", flags);

        paginationProp.Should().NotBeNull();
        dataProp.Should().NotBeNull();

        var pagination = paginationProp!.GetValue(result)!;
        var currentPage = (int)pagination.GetType().GetProperty("currentPage", flags)!.GetValue(pagination)!;
        var totalPages = (int)pagination.GetType().GetProperty("totalPages", flags)!.GetValue(pagination)!;
        var totalItems = (int)pagination.GetType().GetProperty("totalItems", flags)!.GetValue(pagination)!;
        var pageSize = (int)pagination.GetType().GetProperty("pageSize", flags)!.GetValue(pagination)!;

        currentPage.Should().Be(2);
        totalPages.Should().Be(0);
        totalItems.Should().Be(0);
        pageSize.Should().Be(5);

        var dataObj = dataProp!.GetValue(result)!;
        var enumerable = (System.Collections.IEnumerable)dataObj;
        enumerable.Cast<object>().Count().Should().Be(0);
    }

    [Fact]
    public async Task Has_Friends_Calls_BlogPostService_With_Ids_And_Pagination()
    {
        var service = CreateService(out var userClient, out var blogPostService, out var jwt);
        jwt.Setup(j => j.GetCurrentUserId()).Returns("me");
        userClient.Setup(u => u.GetUserFriendsAsync("tok")).ReturnsAsync(new[]
        {
            new FriendDto { UserId = "u1", Name = "A" },
            new FriendDto { UserId = "u2", Name = "B" },
        });

        blogPostService
            .Setup(b => b.GetFriendsPostsAsync(It.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "u1", "u2" })), 3, 20))
            .ReturnsAsync(new PaginatedResult<BlogPostDto> { Data = new List<BlogPostDto>() });

        var ctx = CreateHttpContext("tok");
        var result = await service.GetFriendsPostsAsync(ctx, 3, 20);

        blogPostService.VerifyAll();
        result.Should().BeOfType<PaginatedResult<BlogPostDto>>();
    }
}


