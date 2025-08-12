using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Models.Entities;
using AdminService.Services.Implementations;
using AdminService.Tests.TestUtilities;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Services;


public class AuditServiceTests : IDisposable
{
    private readonly AdminDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AuditService _service;

    public AuditServiceTests()
    {
        _context = MockDbContextFactory.CreateInMemoryContext();
        _mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<AuditService>>();

        _service = new AuditService(
            _context,
            _mockMapper.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task LogActionAsync_WhenValidData_CreatesAuditLog()
    {
        var adminId = "admin-123";
        var action = "Login";
        var targetType = "User";
        var targetId = "user-123";
        var details = new { LoginTime = DateTime.UtcNow };
        var ipAddress = "192.168.1.1";
        var userAgent = "Test Browser";

        await _service.LogActionAsync(adminId, action, targetType, targetId, details, ipAddress, userAgent);

        var auditLog = await _context.AdminAuditLogs
            .FirstOrDefaultAsync(a => a.AdminId == adminId && a.Action == action);

        auditLog.Should().NotBeNull();
        auditLog!.AdminId.Should().Be(adminId);
        auditLog.Action.Should().Be(action);
        auditLog.TargetType.Should().Be(targetType);
        auditLog.TargetId.Should().Be(targetId);
        auditLog.IpAddress.Should().Be(ipAddress);
        auditLog.UserAgent.Should().Be(userAgent);
        auditLog.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task GetAuditLogsAsync_WhenLogsExist_ReturnsPagedResults()
    {
        var adminId = "admin-123";
        await SeedAuditLogs(adminId);

        var expectedDto = new AdminAuditLogDto
        {
            Id = Guid.NewGuid(),
            AdminId = adminId,
            Action = "Login",
            TargetType = "User",
            TargetId = "user-123",
            Details = "Test login",
            Timestamp = DateTime.UtcNow,
            IpAddress = "192.168.1.1"
        };

        _mockMapper.Setup(x => x.Map<List<AdminAuditLogDto>>(It.IsAny<List<AdminAuditLog>>()))
            .Returns([expectedDto]);

        var result = await _service.GetAuditLogsAsync(1, 10, adminId);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().Should().BeEquivalentTo(expectedDto);
        result.TotalCount.Should().BeGreaterThan(0);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WhenNoLogsExist_ReturnsEmptyResults()
    {
        _mockMapper.Setup(x => x.Map<List<AdminAuditLogDto>>(It.IsAny<List<AdminAuditLog>>()))
            .Returns([]);

        var result = await _service.GetAuditLogsAsync(1, 10);

        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    private async Task SeedAuditLogs(string adminId)
    {
        var auditLog = new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            AdminId = adminId,
            Action = "Login",
            TargetType = "User",
            TargetId = "user-123",
            Details = "Test login",
            Timestamp = DateTime.UtcNow,
            IpAddress = "192.168.1.1",
            UserAgent = "Test Browser"
        };

        _context.AdminAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}