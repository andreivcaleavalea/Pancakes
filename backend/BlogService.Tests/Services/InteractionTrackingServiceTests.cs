using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class InteractionTrackingServiceTests
{
    private static InteractionTrackingService CreateService(out Mock<IUserInterestService> userInterestService)
    {
        userInterestService = new Mock<IUserInterestService>(MockBehavior.Strict);
        var logger = new Mock<ILogger<InteractionTrackingService>>();
        return new InteractionTrackingService(userInterestService.Object, logger.Object);
    }

    [Fact]
    public async Task TrackViewAsync_ValidInput_CallsUserInterestService()
    {
        var service = CreateService(out var userInterestService);
        var userId = "user123";
        var tags = new List<string> { "technology", "programming" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "view"))
            .Returns(Task.CompletedTask);

        await service.TrackViewAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "view"), Times.Once);
    }

    [Fact]
    public async Task TrackViewAsync_EmptyTags_CallsUserInterestService()
    {
        var service = CreateService(out var userInterestService);
        var userId = "user123";
        var tags = new List<string>();

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "view"))
            .Returns(Task.CompletedTask);

        await service.TrackViewAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "view"), Times.Once);
    }

    [Fact]
    public async Task TrackViewAsync_ServiceThrowsException_HandlesGracefully()
    {
        var service = CreateService(out var userInterestService);
        var userId = "user123";
        var tags = new List<string> { "technology" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "view"))
            .ThrowsAsync(new Exception("Database error"));

        // Should not throw
        await service.TrackViewAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "view"), Times.Once);
    }

    [Fact]
    public async Task TrackSaveAsync_ValidInput_CallsUserInterestService()
    {
        var service = CreateService(out var userInterestService);
        var userId = "user456";
        var tags = new List<string> { "science", "research" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "save"))
            .Returns(Task.CompletedTask);

        await service.TrackSaveAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "save"), Times.Once);
    }

    [Fact]
    public async Task TrackSaveAsync_ServiceThrowsException_HandlesGracefully()
    {
        var service = CreateService(out var userInterestService);
        var userId = "user456";
        var tags = new List<string> { "science" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "save"))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        // Should not throw
        await service.TrackSaveAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "save"), Times.Once);
    }

    [Fact]
    public async Task TrackRatingAsync_ValidInput_CallsUserInterestServiceWithRating()
    {
        var service = CreateService(out var userInterestService);
        var userId = "user789";
        var tags = new List<string> { "tutorial", "beginner" };
        const double rating = 4.5;

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "rate", rating))
            .Returns(Task.CompletedTask);

        await service.TrackRatingAsync(userId, tags, rating);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "rate", rating), Times.Once);
    }

    [Fact]
    public async Task TrackRatingAsync_ServiceThrowsException_HandlesGracefully()
    {
        var service = CreateService(out var userInterestService);
        var userId = "user789";
        var tags = new List<string> { "tutorial" };
        const double rating = 3.0;

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "rate", rating))
            .ThrowsAsync(new ArgumentException("Invalid rating"));

        // Should not throw
        await service.TrackRatingAsync(userId, tags, rating);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "rate", rating), Times.Once);
    }

    [Fact]
    public async Task TrackCommentAsync_ValidInput_CallsUserInterestService()
    {
        var service = CreateService(out var userInterestService);
        var userId = "commenter1";
        var tags = new List<string> { "discussion", "community" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "comment"))
            .Returns(Task.CompletedTask);

        await service.TrackCommentAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "comment"), Times.Once);
    }

    [Fact]
    public async Task TrackCommentAsync_ServiceThrowsException_HandlesGracefully()
    {
        var service = CreateService(out var userInterestService);
        var userId = "commenter1";
        var tags = new List<string> { "discussion" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "comment"))
            .ThrowsAsync(new TimeoutException("Request timeout"));

        // Should not throw
        await service.TrackCommentAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "comment"), Times.Once);
    }

    [Fact]
    public async Task TrackShareAsync_ValidInput_CallsUserInterestService()
    {
        var service = CreateService(out var userInterestService);
        var userId = "sharer1";
        var tags = new List<string> { "viral", "trending" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "share"))
            .Returns(Task.CompletedTask);

        await service.TrackShareAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "share"), Times.Once);
    }

    [Fact]
    public async Task TrackShareAsync_ServiceThrowsException_HandlesGracefully()
    {
        var service = CreateService(out var userInterestService);
        var userId = "sharer1";
        var tags = new List<string> { "viral" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "share"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Should not throw
        await service.TrackShareAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "share"), Times.Once);
    }

    [Fact]
    public async Task TrackMultipleInteractions_AllSucceed()
    {
        var service = CreateService(out var userInterestService);
        var userId = "activeUser";
        var tags = new List<string> { "technology", "innovation" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "view"))
            .Returns(Task.CompletedTask);
        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "save"))
            .Returns(Task.CompletedTask);
        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "rate", 5.0))
            .Returns(Task.CompletedTask);
        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "comment"))
            .Returns(Task.CompletedTask);
        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "share"))
            .Returns(Task.CompletedTask);

        await service.TrackViewAsync(userId, tags);
        await service.TrackSaveAsync(userId, tags);
        await service.TrackRatingAsync(userId, tags, 5.0);
        await service.TrackCommentAsync(userId, tags);
        await service.TrackShareAsync(userId, tags);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "view"), Times.Once);
        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "save"), Times.Once);
        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "rate", 5.0), Times.Once);
        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "comment"), Times.Once);
        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "share"), Times.Once);
    }

    [Fact]
    public async Task TrackRatingAsync_EdgeRatingValues_CallsService()
    {
        var service = CreateService(out var userInterestService);
        var userId = "rater";
        var tags = new List<string> { "test" };

        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "rate", 0.0))
            .Returns(Task.CompletedTask);
        userInterestService.Setup(u => u.RecordInteractionAsync(userId, tags, "rate", 5.0))
            .Returns(Task.CompletedTask);

        await service.TrackRatingAsync(userId, tags, 0.0);
        await service.TrackRatingAsync(userId, tags, 5.0);

        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "rate", 0.0), Times.Once);
        userInterestService.Verify(u => u.RecordInteractionAsync(userId, tags, "rate", 5.0), Times.Once);
    }
}
