using BlogService.Controllers;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BlogService.Tests.Controllers;

public class ReportsControllerTests
{
    private static ReportsController CreateController(out Mock<IReportService> reportService)
    {
        reportService = new Mock<IReportService>(MockBehavior.Strict);
        var logger = new Mock<ILogger<ReportsController>>();
        var controller = new ReportsController(reportService.Object, logger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    private static void SetupUserClaims(ControllerBase controller, string userId = "user123", string userName = "Test User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName)
        };
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    [Fact]
    public async Task GetReports_WithoutStatus_Returns_Ok()
    {
        var controller = CreateController(out var reportService);
        var reports = new List<ReportDto> { new() { Id = Guid.NewGuid() } };

        reportService.Setup(s => s.GetAllAsync(1, 20))
            .ReturnsAsync(reports);

        var result = await controller.GetReports();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(reports);
    }

    [Fact]
    public async Task GetReports_WithStatus_Returns_Ok()
    {
        var controller = CreateController(out var reportService);
        var reports = new List<ReportDto> { new() { Id = Guid.NewGuid(), Status = ReportStatus.Pending } };
        const ReportStatus status = ReportStatus.Pending;

        reportService.Setup(s => s.GetByStatusAsync(status, 1, 20))
            .ReturnsAsync(reports);

        var result = await controller.GetReports(1, 20, status);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(reports);
    }

    [Fact]
    public async Task GetReports_WithCustomPagination_CallsServiceCorrectly()
    {
        var controller = CreateController(out var reportService);
        var reports = new List<ReportDto>();

        reportService.Setup(s => s.GetAllAsync(3, 50))
            .ReturnsAsync(reports);

        var result = await controller.GetReports(3, 50);

        result.Result.Should().BeOfType<OkObjectResult>();
        reportService.Verify(s => s.GetAllAsync(3, 50), Times.Once);
    }

    [Fact]
    public async Task GetReports_ServiceThrowsException_Returns_500()
    {
        var controller = CreateController(out var reportService);

        reportService.Setup(s => s.GetAllAsync(1, 20))
            .ThrowsAsync(new Exception("Database error"));

        var result = await controller.GetReports();

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetReport_Found_Returns_Ok()
    {
        var controller = CreateController(out var reportService);
        var reportId = Guid.NewGuid();
        var report = new ReportDto { Id = reportId };

        reportService.Setup(s => s.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        var result = await controller.GetReport(reportId);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(report);
    }

    [Fact]
    public async Task GetReport_NotFound_Returns_NotFound()
    {
        var controller = CreateController(out var reportService);
        var reportId = Guid.NewGuid();

        reportService.Setup(s => s.GetByIdAsync(reportId))
            .ReturnsAsync((ReportDto?)null);

        var result = await controller.GetReport(reportId);

        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Report not found");
    }

    [Fact]
    public async Task GetReport_ServiceThrowsException_Returns_500()
    {
        var controller = CreateController(out var reportService);
        var reportId = Guid.NewGuid();

        reportService.Setup(s => s.GetByIdAsync(reportId))
            .ThrowsAsync(new Exception("Database error"));

        var result = await controller.GetReport(reportId);

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CreateReport_NoUserId_Returns_Unauthorized()
    {
        var controller = CreateController(out _);
        var createReportDto = new CreateReportDto();

        var result = await controller.CreateReport(createReportDto);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("User ID not found in token");
    }

    [Fact]
    public async Task CreateReport_CannotReport_Returns_BadRequest()
    {
        var controller = CreateController(out var reportService);
        SetupUserClaims(controller);
        var createReportDto = new CreateReportDto { ContentId = Guid.NewGuid(), ContentType = ReportContentType.Post };
        const string errorMessage = "Cannot report this content";

        reportService.Setup(s => s.ValidateReportAsync("user123", createReportDto.ContentId, createReportDto.ContentType))
            .ReturnsAsync((false, errorMessage));

        var result = await controller.CreateReport(createReportDto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        var responseValue = badRequestResult.Value;
        responseValue.Should().NotBeNull();
        responseValue!.GetType().GetProperty("message")!.GetValue(responseValue).Should().Be(errorMessage);
    }

    [Fact]
    public async Task CreateReport_ValidRequest_Returns_CreatedAt()
    {
        var controller = CreateController(out var reportService);
        SetupUserClaims(controller);
        var createReportDto = new CreateReportDto { ContentId = Guid.NewGuid(), ContentType = ReportContentType.Post };
        var createdReport = new ReportDto { Id = Guid.NewGuid() };

        reportService.Setup(s => s.ValidateReportAsync("user123", createReportDto.ContentId, createReportDto.ContentType))
            .ReturnsAsync((true, null));
        reportService.Setup(s => s.CreateReportAsync(createReportDto, "user123", "Test User"))
            .ReturnsAsync(createdReport);

        var result = await controller.CreateReport(createReportDto);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result.Result!;
        createdResult.ActionName.Should().Be(nameof(ReportsController.GetReport));
        createdResult.Value.Should().BeEquivalentTo(createdReport);
    }

    [Fact]
    public async Task CreateReport_InvalidOperationException_Returns_BadRequest()
    {
        var controller = CreateController(out var reportService);
        SetupUserClaims(controller);
        var createReportDto = new CreateReportDto { ContentId = Guid.NewGuid(), ContentType = ReportContentType.Post };

        reportService.Setup(s => s.ValidateReportAsync("user123", createReportDto.ContentId, createReportDto.ContentType))
            .ReturnsAsync((true, null));
        reportService.Setup(s => s.CreateReportAsync(createReportDto, "user123", "Test User"))
            .ThrowsAsync(new InvalidOperationException("Invalid operation"));

        var result = await controller.CreateReport(createReportDto);

        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid operation");
    }

    [Fact]
    public async Task CreateReport_ArgumentException_Returns_BadRequest()
    {
        var controller = CreateController(out var reportService);
        SetupUserClaims(controller);
        var createReportDto = new CreateReportDto { ContentId = Guid.NewGuid(), ContentType = ReportContentType.Post };

        reportService.Setup(s => s.ValidateReportAsync("user123", createReportDto.ContentId, createReportDto.ContentType))
            .ReturnsAsync((true, null));
        reportService.Setup(s => s.CreateReportAsync(createReportDto, "user123", "Test User"))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        var result = await controller.CreateReport(createReportDto);

        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid argument");
    }

    [Fact]
    public async Task UpdateReport_NoAdminId_Returns_Unauthorized()
    {
        var controller = CreateController(out _);
        var updateReportDto = new UpdateReportDto();

        var result = await controller.UpdateReport(Guid.NewGuid(), updateReportDto);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Admin ID not found in token");
    }

    [Fact]
    public async Task UpdateReport_ValidRequest_Returns_Ok()
    {
        var controller = CreateController(out var reportService);
        SetupUserClaims(controller, "admin123", "Admin User");
        var reportId = Guid.NewGuid();
        var updateReportDto = new UpdateReportDto();
        var updatedReport = new ReportDto { Id = reportId };

        reportService.Setup(s => s.UpdateReportAsync(reportId, updateReportDto))
            .ReturnsAsync(updatedReport);

        var result = await controller.UpdateReport(reportId, updateReportDto);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(updatedReport);
    }

    [Fact]
    public async Task UpdateReport_ArgumentException_Returns_NotFound()
    {
        var controller = CreateController(out var reportService);
        SetupUserClaims(controller, "admin123", "Admin User");
        var reportId = Guid.NewGuid();
        var updateReportDto = new UpdateReportDto();

        reportService.Setup(s => s.UpdateReportAsync(reportId, updateReportDto))
            .ThrowsAsync(new ArgumentException("Report not found"));

        var result = await controller.UpdateReport(reportId, updateReportDto);

        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Report not found");
    }

    [Fact]
    public async Task DeleteReport_ValidRequest_Returns_NoContent()
    {
        var controller = CreateController(out var reportService);
        var reportId = Guid.NewGuid();

        reportService.Setup(s => s.DeleteReportAsync(reportId))
            .Returns(Task.CompletedTask);

        var result = await controller.DeleteReport(reportId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteReport_ServiceThrowsException_Returns_500()
    {
        var controller = CreateController(out var reportService);
        var reportId = Guid.NewGuid();

        reportService.Setup(s => s.DeleteReportAsync(reportId))
            .ThrowsAsync(new Exception("Delete failed"));

        var result = await controller.DeleteReport(reportId);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetReportStats_ValidRequest_Returns_Ok()
    {
        var controller = CreateController(out var reportService);
        var stats = new { TotalReports = 10, PendingReports = 5 };

        reportService.Setup(s => s.GetReportStatsAsync())
            .ReturnsAsync(stats);

        var result = await controller.GetReportStats();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(stats);
    }

    [Fact]
    public async Task GetMyReports_NoUserId_Returns_Unauthorized()
    {
        var controller = CreateController(out _);

        var result = await controller.GetMyReports();

        result.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("User ID not found in token");
    }

    [Fact]
    public async Task GetMyReports_ValidRequest_Returns_Ok()
    {
        var controller = CreateController(out var reportService);
        SetupUserClaims(controller);
        var reports = new List<ReportDto> { new() { Id = Guid.NewGuid() } };

        reportService.Setup(s => s.GetByReporterIdAsync("user123"))
            .ReturnsAsync(reports);

        var result = await controller.GetMyReports();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(reports);
    }

    [Fact]
    public async Task CanReportContent_NoUserId_Returns_Unauthorized()
    {
        var controller = CreateController(out _);

        var result = await controller.CanReportContent(ReportContentType.Post, Guid.NewGuid());

        result.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("User ID not found in token");
    }

    [Fact]
    public async Task CanReportContent_ValidRequest_Returns_Ok()
    {
        var controller = CreateController(out var reportService);
        SetupUserClaims(controller);
        var contentId = Guid.NewGuid();
        const ReportContentType contentType = ReportContentType.Post;

        reportService.Setup(s => s.CanUserReportContentAsync("user123", contentId, contentType))
            .ReturnsAsync(true);

        var result = await controller.CanReportContent(contentType, contentId);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().Be(true);
    }
}
