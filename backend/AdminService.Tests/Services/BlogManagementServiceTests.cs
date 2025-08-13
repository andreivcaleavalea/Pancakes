using AdminService.Services.Implementations;
using AdminService.Services.Interfaces;
using AdminService.Clients.BlogClient.Services;
using AdminService.Clients.BlogClient.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Models.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
using Xunit;
using Moq.Protected;
using System.Net;

namespace AdminService.Tests.Services;

public class BlogManagementServiceTests
{
    private readonly Mock<IBlogServiceClient> _mockBlogServiceClient;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<BlogManagementService>> _mockLogger;
    private readonly BlogManagementService _service;

    private const string TestAdminId = "admin-123";
    private const string TestBlogPostId = "blog-post-456";
    private const string TestIpAddress = "192.168.1.1";
    private const string TestUserAgent = "Mozilla/5.0 Test Browser";

    public BlogManagementServiceTests()
    {
        _mockBlogServiceClient = new Mock<IBlogServiceClient>();
        _mockAuditService = new Mock<IAuditService>();
        _mockLogger = new Mock<ILogger<BlogManagementService>>();

        _service = new BlogManagementService(
            _mockBlogServiceClient.Object,
            _mockAuditService.Object,
            _mockLogger.Object);
    }


    [Fact]
    public async Task SearchBlogPostsAsync_WithValidRequest_ReturnsSuccessResult()
    {
        var request = new BlogPostSearchRequest
        {
            Search = "test search",
            Page = 1,
            PageSize = 10
        };

        var expectedBlogPosts = new PagedResponse<BlogPostDTO>
        {
            Data = new List<BlogPostDTO>
            {
                new() { Id = "1", Title = "Test Post 1", AuthorId = "author1" },
                new() { Id = "2", Title = "Test Post 2", AuthorId = "author2" }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10
        };

        _mockBlogServiceClient
            .Setup(x => x.SearchBlogPostsAsync(request))
            .ReturnsAsync(expectedBlogPosts);

        var result = await _service.SearchBlogPostsAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedBlogPosts);
        result.Message.Should().Be("Blog posts retrieved successfully");

        _mockBlogServiceClient.Verify(x => x.SearchBlogPostsAsync(request), Times.Once);
    }

    [Fact]
    public async Task SearchBlogPostsAsync_WhenExceptionThrown_ReturnsFailureResult()
    {
        var request = new BlogPostSearchRequest { Search = "test" };
        var expectedException = new HttpRequestException("Network error");

        _mockBlogServiceClient
            .Setup(x => x.SearchBlogPostsAsync(request))
            .ThrowsAsync(expectedException);

        var result = await _service.SearchBlogPostsAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while searching blog posts");
        result.Errors.Should().Contain(expectedException.Message);
        result.Data.Should().BeNull();

        VerifyLogError("Error searching blog posts");
    }

    [Fact]
    public async Task SearchBlogPostsAsync_WithEmptyRequest_StillCallsClient()
    {
        var request = new BlogPostSearchRequest();
        var expectedBlogPosts = new PagedResponse<BlogPostDTO>
        {
            Data = new List<BlogPostDTO>(),
            TotalCount = 0
        };

        _mockBlogServiceClient
            .Setup(x => x.SearchBlogPostsAsync(request))
            .ReturnsAsync(expectedBlogPosts);

        var result = await _service.SearchBlogPostsAsync(request);

        result.Success.Should().BeTrue();
        _mockBlogServiceClient.Verify(x => x.SearchBlogPostsAsync(request), Times.Once);
    }



    [Fact]
    public async Task DeleteBlogPostAsync_WhenSuccessful_ReturnsSuccessAndLogsAudit()
    {
        var request = new DeleteBlogPostRequest
        {
            BlogPostId = TestBlogPostId,
            Reason = "Inappropriate content found"
        };

        _mockBlogServiceClient
            .Setup(x => x.DeleteBlogPostAsync(TestBlogPostId, TestAdminId))
            .ReturnsAsync(true);

        _mockAuditService
            .Setup(x => x.LogActionAsync(
                TestAdminId,
                "DELETE_BLOG_POST",
                "BlogPost",
                TestBlogPostId,
                It.IsAny<object>(),
                TestIpAddress,
                TestUserAgent))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteBlogPostAsync(TestBlogPostId, request, TestAdminId, TestIpAddress, TestUserAgent);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be("Blog post deleted successfully");

        _mockBlogServiceClient.Verify(x => x.DeleteBlogPostAsync(TestBlogPostId, TestAdminId), Times.Once);
        _mockAuditService.Verify(x => x.LogActionAsync(
            TestAdminId,
            "DELETE_BLOG_POST",
            "BlogPost",
            TestBlogPostId,
            It.Is<object>(o => o.ToString()!.Contains(request.Reason)),
            TestIpAddress,
            TestUserAgent), Times.Once);
    }

    [Fact]
    public async Task DeleteBlogPostAsync_WhenClientReturnsFalse_ReturnsFailureResult()
    {
        var request = new DeleteBlogPostRequest
        {
            BlogPostId = TestBlogPostId,
            Reason = "Test reason"
        };

        _mockBlogServiceClient
            .Setup(x => x.DeleteBlogPostAsync(TestBlogPostId, TestAdminId))
            .ReturnsAsync(false);

        var result = await _service.DeleteBlogPostAsync(TestBlogPostId, request, TestAdminId, TestIpAddress, TestUserAgent);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to delete blog post");

        _mockBlogServiceClient.Verify(x => x.DeleteBlogPostAsync(TestBlogPostId, TestAdminId), Times.Once);
        _mockAuditService.Verify(x => x.LogActionAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBlogPostAsync_WhenExceptionThrown_ReturnsFailureResult()
    {
        var request = new DeleteBlogPostRequest
        {
            BlogPostId = TestBlogPostId,
            Reason = "Test reason"
        };
        var expectedException = new Exception("Database connection failed");

        _mockBlogServiceClient
            .Setup(x => x.DeleteBlogPostAsync(TestBlogPostId, TestAdminId))
            .ThrowsAsync(expectedException);

        var result = await _service.DeleteBlogPostAsync(TestBlogPostId, request, TestAdminId, TestIpAddress, TestUserAgent);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while deleting the blog post");
        result.Errors.Should().Contain(expectedException.Message);

        VerifyLogError($"Error deleting blog post {TestBlogPostId}");
        _mockAuditService.Verify(x => x.LogActionAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }



    [Fact]
    public async Task UpdateBlogPostStatusAsync_WhenSuccessful_ReturnsSuccessAndLogsAudit()
    {
        var request = new UpdateBlogPostStatusRequest
        {
            BlogPostId = TestBlogPostId,
            Status = 1, // Published
            Reason = "Content approved for publication"
        };

        _mockBlogServiceClient
            .Setup(x => x.UpdateBlogPostStatusAsync(TestBlogPostId, request.Status, TestAdminId))
            .ReturnsAsync(true);

        _mockAuditService
            .Setup(x => x.LogActionAsync(
                TestAdminId,
                "UPDATE_BLOG_POST_STATUS",
                "BlogPost",
                TestBlogPostId,
                It.IsAny<object>(),
                TestIpAddress,
                TestUserAgent))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateBlogPostStatusAsync(TestBlogPostId, request, TestAdminId, TestIpAddress, TestUserAgent);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be("Blog post status updated to Published successfully");

        _mockBlogServiceClient.Verify(x => x.UpdateBlogPostStatusAsync(TestBlogPostId, request.Status, TestAdminId), Times.Once);
        _mockAuditService.Verify(x => x.LogActionAsync(
            TestAdminId,
            "UPDATE_BLOG_POST_STATUS",
            "BlogPost",
            TestBlogPostId,
            It.Is<object>(o => o.ToString()!.Contains(request.Status.ToString()) && o.ToString()!.Contains(request.Reason)),
            TestIpAddress,
            TestUserAgent), Times.Once);
    }

    [Theory]
    [InlineData(0, "Draft")]
    [InlineData(1, "Published")]
    [InlineData(2, "Deleted")]
    [InlineData(99, "Unknown")]
    public async Task UpdateBlogPostStatusAsync_WithDifferentStatuses_ReturnsCorrectStatusName(int status, string expectedStatusName)
    {
        var request = new UpdateBlogPostStatusRequest
        {
            BlogPostId = TestBlogPostId,
            Status = status,
            Reason = "Status change test"
        };

        _mockBlogServiceClient
            .Setup(x => x.UpdateBlogPostStatusAsync(TestBlogPostId, status, TestAdminId))
            .ReturnsAsync(true);

        var result = await _service.UpdateBlogPostStatusAsync(TestBlogPostId, request, TestAdminId, TestIpAddress, TestUserAgent);

        result.Success.Should().BeTrue();
        result.Data.Should().Be($"Blog post status updated to {expectedStatusName} successfully");
    }

    [Fact]
    public async Task UpdateBlogPostStatusAsync_WhenClientReturnsFalse_ReturnsFailureResult()
    {
        var request = new UpdateBlogPostStatusRequest
        {
            BlogPostId = TestBlogPostId,
            Status = 1,
            Reason = "Test reason"
        };

        _mockBlogServiceClient
            .Setup(x => x.UpdateBlogPostStatusAsync(TestBlogPostId, request.Status, TestAdminId))
            .ReturnsAsync(false);

        var result = await _service.UpdateBlogPostStatusAsync(TestBlogPostId, request, TestAdminId, TestIpAddress, TestUserAgent);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to update blog post status");

        _mockAuditService.Verify(x => x.LogActionAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBlogPostStatusAsync_WhenExceptionThrown_ReturnsFailureResult()
    {
        var request = new UpdateBlogPostStatusRequest
        {
            BlogPostId = TestBlogPostId,
            Status = 1,
            Reason = "Test reason"
        };
        var expectedException = new TimeoutException("Request timeout");

        _mockBlogServiceClient
            .Setup(x => x.UpdateBlogPostStatusAsync(TestBlogPostId, request.Status, TestAdminId))
            .ThrowsAsync(expectedException);

        var result = await _service.UpdateBlogPostStatusAsync(TestBlogPostId, request, TestAdminId, TestIpAddress, TestUserAgent);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while updating the blog post status");
        result.Errors.Should().Contain(expectedException.Message);

        VerifyLogError($"Error updating blog post status {TestBlogPostId}");
    }
    
    [Fact]
    public async Task GetBlogStatisticsAsync_WhenSuccessful_ReturnsSuccessResult()
    {
        var expectedStats = new Dictionary<string, object>
        {
            { "totalPosts", 150 },
            { "publishedPosts", 120 },
            { "draftPosts", 25 },
            { "deletedPosts", 5 },
            { "totalAuthors", 15 },
            { "averagePostsPerDay", 2.5 }
        };

        _mockBlogServiceClient
            .Setup(x => x.GetBlogStatisticsAsync())
            .ReturnsAsync(expectedStats);

        var result = await _service.GetBlogStatisticsAsync();

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedStats);
        result.Message.Should().Be("Blog statistics retrieved successfully");

        _mockBlogServiceClient.Verify(x => x.GetBlogStatisticsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBlogStatisticsAsync_WhenExceptionThrown_ReturnsFailureResult()
    {
        var expectedException = new InvalidOperationException("Statistics service unavailable");

        _mockBlogServiceClient
            .Setup(x => x.GetBlogStatisticsAsync())
            .ThrowsAsync(expectedException);

        var result = await _service.GetBlogStatisticsAsync();

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving blog statistics");
        result.Errors.Should().Contain(expectedException.Message);
        result.Data.Should().BeNull();

        VerifyLogError("Error getting blog statistics");
    }

    [Fact]
    public async Task GetBlogStatisticsAsync_WhenReturnsEmptyStats_ReturnsSuccessWithEmptyData()
    {
        var expectedStats = new Dictionary<string, object>();

        _mockBlogServiceClient
            .Setup(x => x.GetBlogStatisticsAsync())
            .ReturnsAsync(expectedStats);

        var result = await _service.GetBlogStatisticsAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
        result.Message.Should().Be("Blog statistics retrieved successfully");
    }
    
    private void VerifyLogError(string expectedMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
