using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AdminService.Clients.BlogClient.DTOs;
using AdminService.Controllers;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AdminService.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Controllers;

public class BlogManagementControllerTests
{
    private readonly Mock<IBlogManagementService> _serviceMock = new();
    private readonly Mock<ILogger<BlogManagementController>> _loggerMock = new();

    private BlogManagementController CreateController()
    {
        var controller = new BlogManagementController(_serviceMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }

    [Fact]
    public async Task SearchBlogPosts_WhenServiceSucceeds_ReturnsOkWithPagedResponse()
    {
        var controller = CreateController();
        var request = new BlogPostSearchRequest { Search = "test", Page = 2, PageSize = 5 };
        var paged = new PagedResponse<BlogPostDTO> { Data = new List<BlogPostDTO>(), CurrentPage = 2, PageSize = 5 };

        _serviceMock
            .Setup(s => s.SearchBlogPostsAsync(request))
            .ReturnsAsync(ServiceResult<PagedResponse<BlogPostDTO>>.SuccessResult(paged, "ok"));

        var result = await controller.SearchBlogPosts(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResponse<BlogPostDTO>>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("ok");
        response.Data.Should().BeSameAs(paged);

        _serviceMock.Verify(s => s.SearchBlogPostsAsync(request), Times.Once);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SearchBlogPosts_WhenServiceFails_Returns500AndLogs()
    {
        var controller = CreateController();
        var request = new BlogPostSearchRequest { Search = "bad" };
        var errors = new List<string> { "e1", "e2" };

        _serviceMock
            .Setup(s => s.SearchBlogPostsAsync(request))
            .ReturnsAsync(ServiceResult<PagedResponse<BlogPostDTO>>.FailureResult("failure", errors));

        var result = await controller.SearchBlogPosts(request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        var response = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("failure");

        _loggerMock.VerifyLogContains(LogLevel.Error, "Error searching blog posts", null, Times.Once());
    }

    [Fact]
    public async Task DeleteBlogPost_WhenValidationFails_ReturnsBadRequestWithErrors()
    {
        var controller = CreateController();
        var request = new DeleteBlogPostRequest { BlogPostId = Guid.NewGuid().ToString(), Reason = "short" };

        var result = await controller.DeleteBlogPost(Guid.NewGuid().ToString(), request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Validation failed");
        response.Errors.Should().NotBeEmpty();

        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteBlogPost_WhenAdminIdMissing_ReturnsUnauthorized()
    {
        var controller = CreateController();
        var request = new DeleteBlogPostRequest { BlogPostId = Guid.NewGuid().ToString(), Reason = new string('a', 12) };

        var result = await controller.DeleteBlogPost(Guid.NewGuid().ToString(), request);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var response = unauthorized.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Admin ID not found in token");

        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteBlogPost_WhenServiceSucceeds_ReturnsOkAndForwardsContext()
    {
        var controller = CreateController();
        var http = controller.ControllerContext.HttpContext!;
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "admin-123") }, "test"));
        http.Request.Headers.UserAgent = "TestAgent";
        http.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        var blogPostId = Guid.NewGuid().ToString();
        var request = new DeleteBlogPostRequest { BlogPostId = blogPostId, Reason = new string('a', 15) };

        _serviceMock
            .Setup(s => s.DeleteBlogPostAsync(
                blogPostId,
                request,
                "admin-123",
                "127.0.0.1",
                "TestAgent"))
            .ReturnsAsync(ServiceResult<string>.SuccessResult("Deleted"));

        var result = await controller.DeleteBlogPost(blogPostId, request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Deleted");

        _serviceMock.VerifyAll();
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteBlogPost_WhenServiceFailsWithKnownMessage_ReturnsBadRequest()
    {
        var controller = CreateController();
        var http = controller.ControllerContext.HttpContext!;
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "admin-123") }, "test"));
        http.Request.Headers.UserAgent = "UA";
        http.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        var blogPostId = Guid.NewGuid().ToString();
        var request = new DeleteBlogPostRequest { BlogPostId = blogPostId, Reason = new string('a', 12) };

        _serviceMock
            .Setup(s => s.DeleteBlogPostAsync(
                blogPostId,
                request,
                "admin-123",
                "10.0.0.1",
                "UA"))
            .ReturnsAsync(ServiceResult<string>.FailureResult("Failed to delete blog post", new List<string> { "e" }));

        var result = await controller.DeleteBlogPost(blogPostId, request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Failed to delete blog post");
    }

    [Fact]
    public async Task DeleteBlogPost_WhenServiceFailsUnknown_Returns500AndLogs()
    {
        var controller = CreateController();
        var http = controller.ControllerContext.HttpContext!;
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "admin-123") }, "test"));

        var blogPostId = Guid.NewGuid().ToString();
        var request = new DeleteBlogPostRequest { BlogPostId = blogPostId, Reason = new string('a', 12) };

        _serviceMock
            .Setup(s => s.DeleteBlogPostAsync(
                blogPostId,
                request,
                "admin-123",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<string>.FailureResult("boom", new List<string> { "e1", "e2" }));

        var result = await controller.DeleteBlogPost(blogPostId, request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        var response = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("boom");

        _loggerMock.VerifyLogContains(LogLevel.Error, "Error deleting blog post", null, Times.Once());
    }

    [Fact]
    public async Task UpdateBlogPostStatus_WhenValidationFails_ReturnsBadRequest()
    {
        var controller = CreateController();
        var blogPostId = Guid.NewGuid().ToString();
        var request = new UpdateBlogPostStatusRequest { BlogPostId = "DIFF", Status = 3, Reason = "short" }; // invalid status and reason

        var result = await controller.UpdateBlogPostStatus(blogPostId, request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Validation failed");
        response.Errors.Should().NotBeEmpty();

        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateBlogPostStatus_WhenAdminIdMissing_ReturnsUnauthorized()
    {
        var controller = CreateController();
        var blogPostId = Guid.NewGuid().ToString();
        var request = new UpdateBlogPostStatusRequest { BlogPostId = "will-be-overwritten", Status = 1, Reason = new string('a', 12) };

        var result = await controller.UpdateBlogPostStatus(blogPostId, request);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var response = unauthorized.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Admin ID not found in token");

        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateBlogPostStatus_WhenServiceSucceeds_ReturnsOkAndOverwritesBodyId()
    {
        var controller = CreateController();
        var http = controller.ControllerContext.HttpContext!;
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "admin-321") }, "test"));
        http.Request.Headers.UserAgent = "AgentX";
        http.Connection.RemoteIpAddress = IPAddress.Parse("192.168.0.10");

        var routeId = Guid.NewGuid().ToString();
        UpdateBlogPostStatusRequest? capturedRequest = null;
        var request = new UpdateBlogPostStatusRequest { BlogPostId = "DIFF-IGNORED", Status = 2, Reason = new string('b', 20) };

        _serviceMock
            .Setup(s => s.UpdateBlogPostStatusAsync(
                routeId,
                It.IsAny<UpdateBlogPostStatusRequest>(),
                "admin-321",
                "192.168.0.10",
                "AgentX"))
            .Callback<string, UpdateBlogPostStatusRequest, string, string, string>((_, r, _, _, _) => capturedRequest = r)
            .ReturnsAsync(ServiceResult<string>.SuccessResult("Updated"));

        var result = await controller.UpdateBlogPostStatus(routeId, request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Updated");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.BlogPostId.Should().Be(routeId);
        capturedRequest.Status.Should().Be(2);
        capturedRequest.Reason.Should().Be(request.Reason);
    }

    [Fact]
    public async Task UpdateBlogPostStatus_WhenServiceFailsKnown_ReturnsBadRequest()
    {
        var controller = CreateController();
        controller.ControllerContext.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "admin-1") }, "test"));

        var routeId = Guid.NewGuid().ToString();
        var request = new UpdateBlogPostStatusRequest { BlogPostId = routeId, Status = 1, Reason = new string('a', 12) };

        _serviceMock
            .Setup(s => s.UpdateBlogPostStatusAsync(
                routeId,
                It.IsAny<UpdateBlogPostStatusRequest>(),
                "admin-1",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<string>.FailureResult("Failed to update blog post status", new List<string> { "e" }));

        var result = await controller.UpdateBlogPostStatus(routeId, request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Failed to update blog post status");
    }

    [Fact]
    public async Task UpdateBlogPostStatus_WhenServiceFailsUnknown_Returns500AndLogs()
    {
        var controller = CreateController();
        controller.ControllerContext.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "admin-1") }, "test"));

        var routeId = Guid.NewGuid().ToString();
        var request = new UpdateBlogPostStatusRequest { BlogPostId = routeId, Status = 1, Reason = new string('a', 12) };

        _serviceMock
            .Setup(s => s.UpdateBlogPostStatusAsync(
                routeId,
                It.IsAny<UpdateBlogPostStatusRequest>(),
                "admin-1",
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<string>.FailureResult("err", new List<string> { "e1", "e2" }));

        var result = await controller.UpdateBlogPostStatus(routeId, request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        var response = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("err");

        _loggerMock.VerifyLogContains(LogLevel.Error, "Error updating blog post status", null, Times.Once());
    }

    [Fact]
    public async Task GetBlogStatistics_WhenServiceSucceeds_ReturnsOk()
    {
        var controller = CreateController();
        var dict = new Dictionary<string, object> { ["posts"] = 10 };

        _serviceMock
            .Setup(s => s.GetBlogStatisticsAsync())
            .ReturnsAsync(ServiceResult<Dictionary<string, object>>.SuccessResult(dict, "ok"));

        var result = await controller.GetBlogStatistics();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<Dictionary<string, object>>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("ok");
        response.Data.Should().BeSameAs(dict);

        _serviceMock.Verify(s => s.GetBlogStatisticsAsync(), Times.Once);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetBlogStatistics_WhenServiceFails_Returns500AndLogs()
    {
        var controller = CreateController();

        _serviceMock
            .Setup(s => s.GetBlogStatisticsAsync())
            .ReturnsAsync(ServiceResult<Dictionary<string, object>>.FailureResult("fail", new List<string> { "e" }));

        var result = await controller.GetBlogStatistics();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        var response = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("fail");

        _loggerMock.VerifyLogContains(LogLevel.Error, "Error getting blog statistics", null, Times.Once());
    }

    [Fact]
    public void Controller_Attributes_AreConfigured()
    {
        var type = typeof(BlogManagementController);
        type.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
        var route = type.GetCustomAttribute<RouteAttribute>();
        route.Should().NotBeNull();
        route!.Template.Should().Be("api/[controller]");
        type.GetCustomAttributes<AuthorizeAttribute>().Should().NotBeEmpty();
    }

    [Fact]
    public void SearchBlogPosts_Attributes_ShouldIncludeHttpGetAndAuthorize()
    {
        var mi = typeof(BlogManagementController).GetMethod(nameof(BlogManagementController.SearchBlogPosts))!;
        mi.Should().NotBeNull();
        var httpGet = mi.GetCustomAttribute<HttpGetAttribute>();
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("search");

        var authorize = mi.GetCustomAttribute<AuthorizeAttribute>();
        authorize.Should().NotBeNull();
        authorize!.Policy.Should().Be("CanViewBlogs");

        var param = mi.GetParameters().Single();
        param.ParameterType.Should().Be(typeof(BlogPostSearchRequest));
        param.GetCustomAttributes(typeof(FromQueryAttribute), false).Should().HaveCount(1);
    }

    [Fact]
    public void DeleteBlogPost_Attributes_ShouldIncludeHttpDeleteAndAuthorize()
    {
        var mi = typeof(BlogManagementController).GetMethod(nameof(BlogManagementController.DeleteBlogPost))!;
        mi.Should().NotBeNull();
        var httpDelete = mi.GetCustomAttribute<HttpDeleteAttribute>();
        httpDelete.Should().NotBeNull();
        httpDelete!.Template.Should().Be("posts/{blogPostId}");

        var authorize = mi.GetCustomAttribute<AuthorizeAttribute>();
        authorize.Should().NotBeNull();
        authorize!.Policy.Should().Be("CanDeleteBlogs");

        var parameters = mi.GetParameters();
        parameters.Length.Should().Be(2);
        parameters[1].GetCustomAttributes(typeof(FromBodyAttribute), false).Should().HaveCount(1);
    }

    [Fact]
    public void UpdateBlogPostStatus_Attributes_ShouldIncludeHttpPutAndAuthorize()
    {
        var mi = typeof(BlogManagementController).GetMethod(nameof(BlogManagementController.UpdateBlogPostStatus))!;
        mi.Should().NotBeNull();
        var httpPut = mi.GetCustomAttribute<HttpPutAttribute>();
        httpPut.Should().NotBeNull();
        httpPut!.Template.Should().Be("posts/{blogPostId}/status");

        var authorize = mi.GetCustomAttribute<AuthorizeAttribute>();
        authorize.Should().NotBeNull();
        authorize!.Policy.Should().Be("CanManageBlogs");

        var parameters = mi.GetParameters();
        parameters.Length.Should().Be(2);
        parameters[1].GetCustomAttributes(typeof(FromBodyAttribute), false).Should().HaveCount(1);
    }

    [Fact]
    public void GetBlogStatistics_Attributes_ShouldIncludeHttpGetAndAuthorize()
    {
        var mi = typeof(BlogManagementController).GetMethod(nameof(BlogManagementController.GetBlogStatistics))!;
        mi.Should().NotBeNull();
        var httpGet = mi.GetCustomAttribute<HttpGetAttribute>();
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("statistics");

        var authorize = mi.GetCustomAttribute<AuthorizeAttribute>();
        authorize.Should().NotBeNull();
        authorize!.Policy.Should().Be("CanViewAnalytics");
    }
}

 


