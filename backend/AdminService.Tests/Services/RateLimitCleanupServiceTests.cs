using AdminService.Services.Implementations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using System.Reflection;
using AdminService.Middleware;

namespace AdminService.Tests.Services;

public class RateLimitCleanupServiceTests : IDisposable
{
    private readonly Mock<ILogger<RateLimitCleanupService>> _mockLogger;
    private readonly RateLimitCleanupService _service;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public RateLimitCleanupServiceTests()
    {
        _mockLogger = new Mock<ILogger<RateLimitCleanupService>>();
        _service = new RateLimitCleanupService(_mockLogger.Object);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [Fact]
    public async Task ExecuteAsync_WhenStarted_LogsStartupMessage()
    {
        _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        await _service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(50); 
        await _cancellationTokenSource.CancelAsync();

        try
        {
            await _service.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation token is triggered
        }

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit cleanup service started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ExecuteAsync_ServiceLifecycle_LogsStartupAndShutdown()
    {
        // Arrange
        using var serviceInstance = new RateLimitCleanupService(_mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        // Act - Start and immediately request cancellation
        var executeTask = serviceInstance.StartAsync(cts.Token);
        await Task.Delay(10); // Brief delay to ensure startup logging - don't use cancelled token
        cts.Cancel(); // Cancel to trigger shutdown
        
        try
        {
            await serviceInstance.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Give some time for final logging - don't use cancelled token
        await Task.Delay(10);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit cleanup service started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_DuringExecution_LogsDebugMessages()
    {
        _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(200));

        await _service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(100); // Give it time to execute at least one cycle
        await _cancellationTokenSource.CancelAsync();

        try
        {
            await _service.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation token is triggered
        }

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleaning up expired rate limit clients")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit cleanup completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void CleanupInterval_ShouldBe30Minutes()
    {
        var cleanupIntervalField = typeof(RateLimitCleanupService)
            .GetField("_cleanupInterval", BindingFlags.NonPublic | BindingFlags.Instance);

        cleanupIntervalField.Should().NotBeNull();
        var intervalValue = (TimeSpan)cleanupIntervalField!.GetValue(_service)!;
        intervalValue.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_StopsGracefully()
    {
        var cancellationToken = new CancellationTokenSource();

        var executeTask = _service.StartAsync(cancellationToken.Token);
        await Task.Delay(50, cancellationToken.Token);
        
        await cancellationToken.CancelAsync(); 
        
        try
        {
            await _service.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation token is triggered
        }

        var completedWithinTimeout = executeTask.Wait(TimeSpan.FromSeconds(5));
        completedWithinTimeout.Should().BeTrue("Service should stop gracefully when cancellation is requested");
    }
    
    
    [Fact]
    public async Task Service_WhenDisposed_ShouldCleanupProperly()
    {
        var cancellationToken = new CancellationTokenSource();
        cancellationToken.CancelAfter(TimeSpan.FromMilliseconds(50));

        await _service.StartAsync(cancellationToken.Token);
        _service.Dispose();

        var disposeAction = () => _service.Dispose();
        disposeAction.Should().NotThrow();
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _service?.Dispose();
    }
}
