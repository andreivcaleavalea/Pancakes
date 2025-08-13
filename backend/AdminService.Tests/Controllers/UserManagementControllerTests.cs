using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdminService.Clients.UserClient;
using AdminService.Clients.UserClient.DTOs;
using AdminService.Controllers;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Controllers;

public class UserManagementControllerTests
{
    private static UserServiceClient CreateUserServiceClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new StubHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
        var logger = new Mock<ILogger<UserServiceClient>>().Object;
        var configuration = new Mock<IConfiguration>().Object;
        var jwt = new Mock<IServiceJwtService>();
        jwt.Setup(j => j.GenerateServiceToken()).Returns("TEST_TOKEN");
        return new UserServiceClient(httpClient, logger, configuration, jwt.Object);
    }

    private static UserManagementController CreateController(UserServiceClient client, Mock<IAuditService> auditMock, Mock<ILogger<UserManagementController>> loggerMock)
    {
        var controller = new UserManagementController(client, auditMock.Object, loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }

    private static void SetAdminIdentity(ControllerBase controller, string adminId = "admin-1")
    {
        controller.ControllerContext.HttpContext!.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, adminId) }, "test"));
        controller.ControllerContext.HttpContext!.Request.Headers.UserAgent = "TestAgent";
        controller.ControllerContext.HttpContext!.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
    }

    [Fact]
    public async Task SearchUsers_WhenClientReturnsData_ReturnsOkWithPagedResponse()
    {
        var client = CreateUserServiceClient(_ =>
        {
            var json = "{\"users\":[],\"totalCount\":1,\"page\":1,\"pageSize\":20,\"totalPages\":1}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);

        var request = new UserSearchRequest { SearchTerm = "a", Page = 1, PageSize = 20 };
        var result = await controller.SearchUsers(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResponse<UserOverviewDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Users retrieved successfully");
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserDetail_WhenFound_ReturnsOk()
    {
        var client = CreateUserServiceClient(req =>
        {
            var json = "{}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);

        var result = await controller.GetUserDetail("user-1");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<UserDetailDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("User details retrieved successfully");
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserDetail_WhenNotFound_Returns404()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);

        var result = await controller.GetUserDetail("missing");

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = notFound.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task BanUser_WhenValidationFails_ReturnsBadRequest()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);

        var request = new BanUserRequest { UserId = "not-a-guid", Reason = "" };
        var result = await controller.BanUser(request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Validation failed");
        response.Errors.Should().NotBeEmpty();
        auditMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task BanUser_WhenClientReturnsFalse_ReturnsBadRequest()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);
        SetAdminIdentity(controller);

        var request = new BanUserRequest { UserId = Guid.NewGuid().ToString(), Reason = new string('a', 12) };
        var result = await controller.BanUser(request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Failed to ban user");
        auditMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task BanUser_WhenClientReturnsTrue_AuditsAndReturnsOk()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);
        SetAdminIdentity(controller, "admin-42");

        var request = new BanUserRequest { UserId = Guid.NewGuid().ToString(), Reason = new string('b', 20) };
        var result = await controller.BanUser(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("User banned successfully");

        auditMock.Verify(a => a.LogActionAsync(
            "admin-42",
            "USER_BANNED",
            "User",
            request.UserId,
            request,
            "127.0.0.1",
            "TestAgent"), Times.Once);
    }

    [Fact]
    public async Task UnbanUser_WhenValidationFails_ReturnsBadRequest()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);

        var request = new UnbanUserRequest { UserId = "", Reason = new string('a', 12) };
        var result = await controller.UnbanUser(request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Validation failed");
        response.Errors.Should().NotBeEmpty();
        auditMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UnbanUser_WhenClientReturnsTrue_AuditsAndReturnsOk()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);
        SetAdminIdentity(controller, "admin-7");

        var request = new UnbanUserRequest { UserId = Guid.NewGuid().ToString(), Reason = new string('a', 12) };
        var result = await controller.UnbanUser(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("User unbanned successfully");

        auditMock.Verify(a => a.LogActionAsync(
            "admin-7",
            "USER_UNBANNED",
            "User",
            request.UserId,
            request,
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UnbanUser_WhenClientReturnsFalse_ReturnsBadRequest()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);
        SetAdminIdentity(controller);

        var request = new UnbanUserRequest { UserId = Guid.NewGuid().ToString(), Reason = new string('a', 12) };
        var result = await controller.UnbanUser(request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Failed to unban user");
    }

    [Fact]
    public async Task UpdateUser_WhenClientReturnsUser_AuditsAndReturnsOk()
    {
        var client = CreateUserServiceClient(_ =>
        {
            var json = "{}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);
        SetAdminIdentity(controller, "admin-upd");

        var result = await controller.UpdateUser("user-1", new UpdateUserRequest { Email = "a@b.com", Name = "n" });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<UserDetailDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("User updated successfully");
        response.Data.Should().NotBeNull();

        auditMock.Verify(a => a.LogActionAsync(
            "admin-upd",
            "USER_UPDATED",
            "User",
            "user-1",
            It.IsAny<UpdateUserRequest>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_WhenClientReturnsNull_ReturnsNotFound()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);
        SetAdminIdentity(controller);

        var result = await controller.UpdateUser("user-1", new UpdateUserRequest { Email = "a@b.com", Name = "n" });

        var nf = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = nf.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("User not found");
        auditMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForcePasswordReset_AlwaysReturnsOkAndAudits()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);
        SetAdminIdentity(controller, "admin-reset");

        var request = new ForcePasswordResetRequest { UserId = Guid.NewGuid().ToString() };
        var result = await controller.ForcePasswordReset(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Password reset initiated successfully");

        auditMock.Verify(a => a.LogActionAsync(
            "admin-reset",
            "FORCE_PASSWORD_RESET",
            "User",
            request.UserId,
            request,
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetUserStatistics_ReturnsOkWithDictionary()
    {
        var client = CreateUserServiceClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var auditMock = new Mock<IAuditService>();
        var loggerMock = new Mock<ILogger<UserManagementController>>();
        var controller = CreateController(client, auditMock, loggerMock);

        var result = await controller.GetUserStatistics();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<Dictionary<string, object>>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("User statistics retrieved successfully");
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public void Controller_Attributes_AreConfigured()
    {
        var type = typeof(UserManagementController);
        type.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
        var route = type.GetCustomAttribute<RouteAttribute>();
        route.Should().NotBeNull();
        route!.Template.Should().Be("api/[controller]");
        type.GetCustomAttributes<AuthorizeAttribute>().Should().NotBeEmpty();
    }

    [Fact]
    public void SearchUsers_Attributes_ShouldIncludeHttpGetAndAuthorize()
    {
        var mi = typeof(UserManagementController).GetMethod(nameof(UserManagementController.SearchUsers))!;
        var httpGet = mi.GetCustomAttribute<HttpGetAttribute>();
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("search");
        mi.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be("CanViewUsers");
        mi.GetParameters().Single().GetCustomAttributes(typeof(FromQueryAttribute), false).Should().HaveCount(1);
    }

    [Fact]
    public void GetUserDetail_Attributes_ShouldIncludeHttpGetAndAuthorize()
    {
        var mi = typeof(UserManagementController).GetMethod(nameof(UserManagementController.GetUserDetail))!;
        var httpGet = mi.GetCustomAttribute<HttpGetAttribute>();
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("users/{userId}");
        mi.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be("CanViewUserDetails");
    }

    [Fact]
    public void BanUser_Attributes_ShouldIncludeHttpPostAndAuthorize()
    {
        var mi = typeof(UserManagementController).GetMethod(nameof(UserManagementController.BanUser))!;
        var httpPost = mi.GetCustomAttribute<HttpPostAttribute>();
        httpPost.Should().NotBeNull();
        httpPost!.Template.Should().Be("ban");
        mi.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be("CanBanUsers");
        mi.GetParameters().Single().GetCustomAttributes(typeof(FromBodyAttribute), false).Should().HaveCount(1);
    }

    [Fact]
    public void UnbanUser_Attributes_ShouldIncludeHttpPostAndAuthorize()
    {
        var mi = typeof(UserManagementController).GetMethod(nameof(UserManagementController.UnbanUser))!;
        var httpPost = mi.GetCustomAttribute<HttpPostAttribute>();
        httpPost.Should().NotBeNull();
        httpPost!.Template.Should().Be("unban");
        mi.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be("CanUnbanUsers");
        mi.GetParameters().Single().GetCustomAttributes(typeof(FromBodyAttribute), false).Should().HaveCount(1);
    }

    [Fact]
    public void UpdateUser_Attributes_ShouldIncludeHttpPutAndAuthorize()
    {
        var mi = typeof(UserManagementController).GetMethod(nameof(UserManagementController.UpdateUser))!;
        var httpPut = mi.GetCustomAttribute<HttpPutAttribute>();
        httpPut.Should().NotBeNull();
        httpPut!.Template.Should().Be("users/{userId}");
        mi.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be("CanUpdateUsers");
        mi.GetParameters()[1].GetCustomAttributes(typeof(FromBodyAttribute), false).Should().HaveCount(1);
    }

    [Fact]
    public void ForcePasswordReset_Attributes_ShouldIncludeHttpPostAndAuthorize()
    {
        var mi = typeof(UserManagementController).GetMethod(nameof(UserManagementController.ForcePasswordReset))!;
        var httpPost = mi.GetCustomAttribute<HttpPostAttribute>();
        httpPost.Should().NotBeNull();
        httpPost!.Template.Should().Be("force-password-reset");
        mi.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be("CanUpdateUsers");
        mi.GetParameters().Single().GetCustomAttributes(typeof(FromBodyAttribute), false).Should().HaveCount(1);
    }

    [Fact]
    public void GetUserStatistics_Attributes_ShouldIncludeHttpGetAndAuthorize()
    {
        var mi = typeof(UserManagementController).GetMethod(nameof(UserManagementController.GetUserStatistics))!;
        var httpGet = mi.GetCustomAttribute<HttpGetAttribute>();
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("statistics");
        mi.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be("CanViewAnalytics");
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}


