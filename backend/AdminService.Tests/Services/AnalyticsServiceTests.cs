using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Services.Implementations;
using AdminService.Tests.TestUtilities;
using AdminService.Clients.UserClient;
using AdminService.Clients.BlogClient.Services;
using AdminService.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Services;

public class AnalyticsServiceTests : IDisposable
{
    private readonly AdminDbContext _context;
    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        _context = MockDbContextFactory.CreateSeededContext();
        
        // Create mock service clients 
        var mockHttpClient = new Mock<HttpClient>();
        var mockUserLogger = new Mock<ILogger<UserServiceClient>>();
        var mockBlogLogger = new Mock<ILogger<BlogServiceClient>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceJwtService = new Mock<IServiceJwtService>();
        
        var mockUserServiceClient = new Mock<UserServiceClient>(
            mockHttpClient.Object,
            mockUserLogger.Object,
            mockConfiguration.Object,
            mockServiceJwtService.Object);
            
        var mockBlogServiceClient = new Mock<BlogServiceClient>(
            mockHttpClient.Object,
            mockBlogLogger.Object,
            mockConfiguration.Object,
            mockServiceJwtService.Object);
        
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockAuditService = new Mock<IAuditService>();
        var mockLogger = new Mock<ILogger<AnalyticsService>>();

        _service = new AnalyticsService(
            _context,
            mockUserServiceClient.Object,
            mockBlogServiceClient.Object,
            mockHttpContextAccessor.Object,
            mockAuditService.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WhenCalled_ReturnsCompleteStats()
    {
        
        var result = await _service.GetDashboardStatsAsync();

        result.Should().NotBeNull();
        result.UserStats.Should().NotBeNull();
        result.ContentStats.Should().NotBeNull();
        result.ModerationStats.Should().NotBeNull();
        result.SystemStats.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDetailedAnalyticsAsync_WhenCalled_ReturnsAnalyticsData()
    {
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;

        var result = await _service.GetDetailedAnalyticsAsync(fromDate, toDate);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDetailedAnalyticsAsync_WhenNoDatesProvided_ReturnsDefaultRange()
    {
        var result = await _service.GetDetailedAnalyticsAsync();

        result.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}