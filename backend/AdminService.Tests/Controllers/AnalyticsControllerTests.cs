using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdminService.Controllers;
using AdminService.Models.DTOs;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly Mock<IAnalyticsService> _analyticsServiceMock = new();
    private readonly Mock<ILogger<AnalyticsController>> _loggerMock = new();
    private readonly AnalyticsController _controller;

    public AnalyticsControllerTests()
    {
        _controller = new AnalyticsController(_analyticsServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetDashboardStats_WhenServiceSucceeds_ReturnsOkWithResponse()
    {
        var expectedStats = new DashboardStatsDto
        {
            UserStats = new UserStatsDto { TotalUsers = 10, ActiveUsers = 5 },
            ContentStats = new ContentStatsDto { TotalBlogPosts = 100 },
            ModerationStats = new ModerationStatsDto(),
            SystemStats = new SystemStatsDto { DatabaseStatus = "Healthy" }
        };

        _analyticsServiceMock
            .Setup(s => s.GetDashboardStatsAsync())
            .ReturnsAsync(expectedStats);

        var result = await _controller.GetDashboardStats();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<DashboardStatsDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Dashboard statistics retrieved successfully");
        response.Data.Should().BeSameAs(expectedStats);

        _analyticsServiceMock.Verify(s => s.GetDashboardStatsAsync(), Times.Once);
        _analyticsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDashboardStats_WhenServiceThrows_Returns500AndLogsError()
    {
        var exception = new InvalidOperationException("boom");
        _analyticsServiceMock
            .Setup(s => s.GetDashboardStatsAsync())
            .ThrowsAsync(exception);

        var result = await _controller.GetDashboardStats();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);

        var response = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("An error occurred while retrieving dashboard statistics");
        response.Data.Should().BeNull();

        _loggerMock.VerifyLogContains(LogLevel.Error, "Error retrieving dashboard statistics", exception, Times.Once());
    }

    [Fact]
    public async Task GetDetailedAnalytics_WithDates_ForwardsParametersAndReturnsOk()
    {
        DateTime? fromDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        DateTime? toDate = new DateTime(2024, 01, 31, 23, 59, 59, DateTimeKind.Utc);
        var expected = new { foo = "bar", count = 3 };

        _analyticsServiceMock
            .Setup(s => s.GetDetailedAnalyticsAsync(fromDate, toDate))
            .ReturnsAsync(expected);

        var result = await _controller.GetDetailedAnalytics(fromDate, toDate);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Detailed analytics retrieved successfully");
        response.Data.Should().BeSameAs(expected);

        _analyticsServiceMock.Verify(s => s.GetDetailedAnalyticsAsync(fromDate, toDate), Times.Once);
        _analyticsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDetailedAnalytics_WithoutDates_ForwardsNullsAndReturnsOk()
    {
        object expected = new();
        _analyticsServiceMock
            .Setup(s => s.GetDetailedAnalyticsAsync(null, null))
            .ReturnsAsync(expected);

        var result = await _controller.GetDetailedAnalytics();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Detailed analytics retrieved successfully");
        response.Data.Should().BeSameAs(expected);

        _analyticsServiceMock.Verify(s => s.GetDetailedAnalyticsAsync(null, null), Times.Once);
        _analyticsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDetailedAnalytics_WhenServiceThrows_Returns500AndLogsError()
    {
        var exception = new ApplicationException("kaboom");
        _analyticsServiceMock
            .Setup(s => s.GetDetailedAnalyticsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(exception);

        var result = await _controller.GetDetailedAnalytics(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);

        var response = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("An error occurred while retrieving detailed analytics");
        response.Data.Should().BeNull();

        _loggerMock.VerifyLogContains(LogLevel.Error, "Error retrieving detailed analytics", exception, Times.Once());
    }

    [Fact]
    public void Controller_Attributes_AreConfigured()
    {
        var controllerType = typeof(AnalyticsController);

        controllerType.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();

        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        routeAttr.Should().NotBeNull();
        routeAttr!.Template.Should().Be("api/[controller]");

        controllerType.GetCustomAttributes<AuthorizeAttribute>().Should().NotBeEmpty();
    }

    [Fact]
    public void GetDashboardStats_Attributes_ShouldIncludeHttpGetAndAuthorizePolicy()
    {
        var methodInfo = typeof(AnalyticsController).GetMethod(nameof(AnalyticsController.GetDashboardStats));
        methodInfo.Should().NotBeNull();

        var httpGet = methodInfo!.GetCustomAttribute<HttpGetAttribute>();
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("dashboard");

        var authorize = methodInfo!.GetCustomAttribute<AuthorizeAttribute>();
        authorize.Should().NotBeNull();
        authorize!.Policy.Should().Be("CanViewDashboard");
    }

    [Fact]
    public void GetDetailedAnalytics_Attributes_ShouldIncludeHttpGetWithRoute()
    {
        var methodInfo = typeof(AnalyticsController).GetMethods()
            .Single(m => m.Name == nameof(AnalyticsController.GetDetailedAnalytics) && m.GetParameters().Length == 2);

        var httpGet = methodInfo.GetCustomAttribute<HttpGetAttribute>();
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("detailed");

        methodInfo.GetCustomAttribute<AuthorizeAttribute>().Should().BeNull();
    }

    [Fact]
    public void GetDetailedAnalytics_Parameters_ShouldHaveFromQueryAndDefaultNull()
    {
        var methodInfo = typeof(AnalyticsController).GetMethods()
            .Single(m => m.Name == nameof(AnalyticsController.GetDetailedAnalytics) && m.GetParameters().Length == 2);

        var parameters = methodInfo.GetParameters();
        parameters.Length.Should().Be(2);

        var fromParam = parameters[0];
        var toParam = parameters[1];

        fromParam.GetCustomAttributes(typeof(FromQueryAttribute), inherit: false).Should().HaveCount(1);
        toParam.GetCustomAttributes(typeof(FromQueryAttribute), inherit: false).Should().HaveCount(1);

        fromParam.HasDefaultValue.Should().BeTrue();
        toParam.HasDefaultValue.Should().BeTrue();

        fromParam.DefaultValue.Should().BeNull();
        toParam.DefaultValue.Should().BeNull();
    }
}

internal static class LoggerMoqExtensions
{
    public static void VerifyLogContains<T>(
        this Mock<ILogger<T>> loggerMock,
        LogLevel level,
        string expectedMessageSubstring,
        Exception? expectedException,
        Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() != null && state.ToString()!.Contains(expectedMessageSubstring)),
                It.Is<Exception?>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}


