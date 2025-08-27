using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using BlogService.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class ReportServiceTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public ReportServiceTests(MappingFixture mapping)
    {
        _mapper = mapping.Mapper;
    }

    private static ReportService CreateService(
        IMapper mapper,
        out Mock<IReportRepository> reportRepo,
        out Mock<IBlogPostRepository> blogPostRepo,
        out Mock<ICommentRepository> commentRepo,
        out Mock<IUserServiceClient> userServiceClient,
        out Mock<IHttpContextAccessor> httpContextAccessor)
    {
        reportRepo = new Mock<IReportRepository>(MockBehavior.Strict);
        blogPostRepo = new Mock<IBlogPostRepository>(MockBehavior.Strict);
        commentRepo = new Mock<ICommentRepository>(MockBehavior.Strict);
        userServiceClient = new Mock<IUserServiceClient>(MockBehavior.Loose);
        httpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);

        var logger = new Mock<ILogger<ReportService>>();
        var cache = new MemoryCache(new MemoryCacheOptions());

        return new ReportService(
            reportRepo.Object,
            blogPostRepo.Object,
            commentRepo.Object,
            mapper,
            logger.Object,
            cache,
            userServiceClient.Object,
            httpContextAccessor.Object);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var service = CreateService(_mapper, out var reportRepo, out _, out _, out _, out _);
        var reportId = Guid.NewGuid();

        reportRepo.Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        var result = await service.GetByIdAsync(reportId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsDto()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var reportId = Guid.NewGuid();
        var contentId = Guid.NewGuid();
        var report = new Report 
        { 
            Id = reportId, 
            ContentId = contentId,
            ContentType = ReportContentType.BlogPost,
            ReporterId = "user1",
            ReporterName = "User One"
        };

        reportRepo.Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);
        blogPostRepo.Setup(b => b.GetByIdAsync(contentId))
            .ReturnsAsync(new BlogPost { Id = contentId, Title = "Test Post", Content = "Test content" });

        var result = await service.GetByIdAsync(reportId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(reportId);
        result.ContentTitle.Should().Be("Test Post");
    }

    [Fact]
    public async Task GetByIdAsync_CachesResult()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var reportId = Guid.NewGuid();
        var contentId = Guid.NewGuid();
        var report = new Report 
        { 
            Id = reportId, 
            ContentId = contentId,
            ContentType = ReportContentType.BlogPost,
            ReporterId = "user1",
            ReporterName = "User One"
        };

        reportRepo.Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);
        blogPostRepo.Setup(b => b.GetByIdAsync(contentId))
            .ReturnsAsync(new BlogPost { Id = contentId, Title = "Test Post", Content = "Test content" });

        // Call twice
        var result1 = await service.GetByIdAsync(reportId);
        var result2 = await service.GetByIdAsync(reportId);

        // Should only call repository once due to caching
        reportRepo.Verify(r => r.GetByIdAsync(reportId), Times.Once);
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var reports = new List<Report>
        {
            new() { Id = Guid.NewGuid(), ContentType = ReportContentType.BlogPost, ContentId = Guid.NewGuid() },
            new() { Id = Guid.NewGuid(), ContentType = ReportContentType.BlogPost, ContentId = Guid.NewGuid() }
        };

        reportRepo.Setup(r => r.GetAllAsync(1, 20))
            .ReturnsAsync(reports);
        
        // Setup blog post lookups
        foreach (var report in reports)
        {
            blogPostRepo.Setup(b => b.GetByIdAsync(report.ContentId))
                .ReturnsAsync(new BlogPost { Id = report.ContentId, Title = "Test", Content = "Content" });
        }

        var result = await service.GetAllAsync(1, 20);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateReportAsync_BlogPost_NotFound_ThrowsArgumentException()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var createDto = new CreateReportDto 
        { 
            ContentId = Guid.NewGuid(), 
            ContentType = ReportContentType.BlogPost,
            Reason = "Spam" 
        };

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", createDto.ContentId, createDto.ContentType))
            .ReturnsAsync(false);
        blogPostRepo.Setup(b => b.GetByIdAsync(createDto.ContentId))
            .ReturnsAsync((BlogPost?)null);

        Func<Task> act = async () => await service.CreateReportAsync(createDto, "user1", "User One");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Blog post not found.");
    }

    [Fact]
    public async Task CreateReportAsync_UserAlreadyReported_ThrowsInvalidOperationException()
    {
        var service = CreateService(_mapper, out var reportRepo, out _, out _, out _, out _);
        var createDto = new CreateReportDto 
        { 
            ContentId = Guid.NewGuid(), 
            ContentType = ReportContentType.BlogPost,
            Reason = "Spam" 
        };

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", createDto.ContentId, createDto.ContentType))
            .ReturnsAsync(true);

        Func<Task> act = async () => await service.CreateReportAsync(createDto, "user1", "User One");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You have already reported this content.");
    }

    [Fact]
    public async Task CreateReportAsync_UserReportsOwnContent_ThrowsInvalidOperationException()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var createDto = new CreateReportDto 
        { 
            ContentId = Guid.NewGuid(), 
            ContentType = ReportContentType.BlogPost,
            Reason = "Spam" 
        };

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", createDto.ContentId, createDto.ContentType))
            .ReturnsAsync(false);
        blogPostRepo.Setup(b => b.GetByIdAsync(createDto.ContentId))
            .ReturnsAsync(new BlogPost { Id = createDto.ContentId, AuthorId = "user1" }); // Same user

        Func<Task> act = async () => await service.CreateReportAsync(createDto, "user1", "User One");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You cannot report your own content.");
    }

    [Fact]
    public async Task CreateReportAsync_ValidBlogPostReport_CreatesReport()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out var userServiceClient, out var httpContextAccessor);
        var createDto = new CreateReportDto 
        { 
            ContentId = Guid.NewGuid(), 
            ContentType = ReportContentType.BlogPost,
            Reason = "Spam",
            Description = "This is spam content"
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer test-token";
        httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", createDto.ContentId, createDto.ContentType))
            .ReturnsAsync(false);
        blogPostRepo.Setup(b => b.GetByIdAsync(createDto.ContentId))
            .ReturnsAsync(new BlogPost { Id = createDto.ContentId, AuthorId = "author1", Title = "Test Post" });
        
        userServiceClient.Setup(u => u.GetUserByIdAsync("author1", "test-token"))
            .ReturnsAsync(new UserInfoDto { Id = "author1", Name = "Author Name" });

        Report? capturedReport = null;
        reportRepo.Setup(r => r.CreateAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .ReturnsAsync((Report r) => { r.Id = Guid.NewGuid(); return r; });

        var result = await service.CreateReportAsync(createDto, "user1", "User One");

        capturedReport.Should().NotBeNull();
        capturedReport!.ReporterId.Should().Be("user1");
        capturedReport.ReporterName.Should().Be("User One");
        capturedReport.ReportedUserId.Should().Be("author1");
        capturedReport.ReportedUserName.Should().Be("Author Name");
        capturedReport.ContentType.Should().Be(ReportContentType.BlogPost);
        capturedReport.ContentId.Should().Be(createDto.ContentId);
        capturedReport.Reason.Should().Be("Spam");
        capturedReport.Description.Should().Be("This is spam content");
        capturedReport.Status.Should().Be(ReportStatus.Pending);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateReportAsync_Comment_CreatesReport()
    {
        var service = CreateService(_mapper, out var reportRepo, out _, out var commentRepo, out _, out _);
        var createDto = new CreateReportDto 
        { 
            ContentId = Guid.NewGuid(), 
            ContentType = ReportContentType.Comment,
            Reason = "Harassment"
        };

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", createDto.ContentId, createDto.ContentType))
            .ReturnsAsync(false);
        commentRepo.Setup(c => c.GetByIdAsync(createDto.ContentId))
            .ReturnsAsync(new Comment 
            { 
                Id = createDto.ContentId, 
                AuthorId = "author1", 
                AuthorName = "Author Name",
                Content = "Test comment"
            });

        Report? capturedReport = null;
        reportRepo.Setup(r => r.CreateAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .ReturnsAsync((Report r) => { r.Id = Guid.NewGuid(); return r; });

        var result = await service.CreateReportAsync(createDto, "user1", "User One");

        capturedReport.Should().NotBeNull();
        capturedReport!.ContentType.Should().Be(ReportContentType.Comment);
        capturedReport.ReportedUserId.Should().Be("author1");
        capturedReport.ReportedUserName.Should().Be("Author Name");
    }

    [Fact]
    public async Task UpdateReportAsync_NotFound_ThrowsArgumentException()
    {
        var service = CreateService(_mapper, out var reportRepo, out _, out _, out _, out _);
        var reportId = Guid.NewGuid();
        var updateDto = new UpdateReportDto { Status = ReportStatus.Resolved };

        reportRepo.Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        Func<Task> act = async () => await service.UpdateReportAsync(reportId, updateDto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Report not found.");
    }

    [Fact]
    public async Task UpdateReportAsync_ValidUpdate_UpdatesReport()
    {
        var service = CreateService(_mapper, out var reportRepo, out _, out _, out _, out _);
        var reportId = Guid.NewGuid();
        var existingReport = new Report { Id = reportId, Status = ReportStatus.Pending };
        var updateDto = new UpdateReportDto 
        { 
            Status = ReportStatus.Resolved,
            AdminNotes = "Resolved by admin",
            UserBanned = true,
            ContentRemoved = false
        };

        reportRepo.Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(existingReport);

        Report? updatedReport = null;
        reportRepo.Setup(r => r.UpdateAsync(It.IsAny<Report>()))
            .Callback<Report>(r => updatedReport = r)
            .ReturnsAsync((Report r) => r);

        var result = await service.UpdateReportAsync(reportId, updateDto);

        updatedReport.Should().NotBeNull();
        updatedReport!.Status.Should().Be(ReportStatus.Resolved);
        updatedReport.AdminNotes.Should().Be("Resolved by admin");
        updatedReport.UserBanned.Should().BeTrue();
        updatedReport.ContentRemoved.Should().BeFalse();
        updatedReport.ReviewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        updatedReport.ReviewedBy.Should().Be("Admin");
    }

    [Fact]
    public async Task DeleteReportAsync_CallsRepository()
    {
        var service = CreateService(_mapper, out var reportRepo, out _, out _, out _, out _);
        var reportId = Guid.NewGuid();

        reportRepo.Setup(r => r.DeleteAsync(reportId))
            .ReturnsAsync(true);

        var result = await service.DeleteReportAsync(reportId);

        result.Should().BeTrue();
        reportRepo.Verify(r => r.DeleteAsync(reportId), Times.Once);
    }

    [Fact]
    public async Task ValidateReportAsync_UserAlreadyReported_ReturnsFalse()
    {
        var service = CreateService(_mapper, out var reportRepo, out _, out _, out _, out _);

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", It.IsAny<Guid>(), It.IsAny<ReportContentType>()))
            .ReturnsAsync(true);

        var result = await service.ValidateReportAsync("user1", Guid.NewGuid(), ReportContentType.BlogPost);

        result.CanReport.Should().BeFalse();
        result.ErrorMessage.Should().Be("You have already reported this content");
    }

    [Fact]
    public async Task ValidateReportAsync_BlogPostNotExists_ReturnsFalse()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var contentId = Guid.NewGuid();

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", contentId, ReportContentType.BlogPost))
            .ReturnsAsync(false);
        blogPostRepo.Setup(b => b.GetByIdAsync(contentId))
            .ReturnsAsync((BlogPost?)null);

        var result = await service.ValidateReportAsync("user1", contentId, ReportContentType.BlogPost);

        result.CanReport.Should().BeFalse();
        result.ErrorMessage.Should().Be("The blog post you're trying to report does not exist");
    }

    [Fact]
    public async Task ValidateReportAsync_UserOwnsContent_ReturnsFalse()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var contentId = Guid.NewGuid();

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", contentId, ReportContentType.BlogPost))
            .ReturnsAsync(false);
        blogPostRepo.Setup(b => b.GetByIdAsync(contentId))
            .ReturnsAsync(new BlogPost { Id = contentId, AuthorId = "user1" });

        var result = await service.ValidateReportAsync("user1", contentId, ReportContentType.BlogPost);

        result.CanReport.Should().BeFalse();
        result.ErrorMessage.Should().Be("You cannot report your own blog post");
    }

    [Fact]
    public async Task ValidateReportAsync_ValidBlogPost_ReturnsTrue()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var contentId = Guid.NewGuid();

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", contentId, ReportContentType.BlogPost))
            .ReturnsAsync(false);
        blogPostRepo.Setup(b => b.GetByIdAsync(contentId))
            .ReturnsAsync(new BlogPost { Id = contentId, AuthorId = "other-user" });

        var result = await service.ValidateReportAsync("user1", contentId, ReportContentType.BlogPost);

        result.CanReport.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateReportAsync_ValidComment_ReturnsTrue()
    {
        var service = CreateService(_mapper, out var reportRepo, out _, out var commentRepo, out _, out _);
        var contentId = Guid.NewGuid();

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", contentId, ReportContentType.Comment))
            .ReturnsAsync(false);
        commentRepo.Setup(c => c.GetByIdAsync(contentId))
            .ReturnsAsync(new Comment { Id = contentId, AuthorId = "other-user" });

        var result = await service.ValidateReportAsync("user1", contentId, ReportContentType.Comment);

        result.CanReport.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task CanUserReportContentAsync_CallsValidateReport()
    {
        var service = CreateService(_mapper, out var reportRepo, out var blogPostRepo, out _, out _, out _);
        var contentId = Guid.NewGuid();

        reportRepo.Setup(r => r.HasUserReportedContentAsync("user1", contentId, ReportContentType.BlogPost))
            .ReturnsAsync(false);
        blogPostRepo.Setup(b => b.GetByIdAsync(contentId))
            .ReturnsAsync(new BlogPost { Id = contentId, AuthorId = "other-user" });

        var result = await service.CanUserReportContentAsync("user1", contentId, ReportContentType.BlogPost);

        result.Should().BeTrue();
    }
}
