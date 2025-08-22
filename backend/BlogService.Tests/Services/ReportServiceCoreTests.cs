using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class ReportServiceCoreTests
{
    private readonly Mock<IReportRepository> _reportRepo = new();
    private readonly Mock<IBlogPostRepository> _blogRepo = new();
    private readonly Mock<ICommentRepository> _commentRepo = new();
    private readonly Mock<IUserServiceClient> _userClient = new();
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly Mock<ILogger<ReportService>> _logger = new();
    private readonly DefaultHttpContext _ctx = new();

    public ReportServiceCoreTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Report, ReportDto>();
        });
        _mapper = cfg.CreateMapper();
        _ctx.Request.Headers.Authorization = "Bearer testtoken";
    }

    private ReportService CreateService() => new(
        _reportRepo.Object,
        _blogRepo.Object,
        _commentRepo.Object,
        _mapper,
        _logger.Object,
        _cache,
        _userClient.Object,
        new HttpContextAccessor { HttpContext = _ctx });

    [Fact]
    public async Task CreateReportAsync_ForBlogPost_Succeeds()
    {
        var svc = CreateService();
        var postId = Guid.NewGuid();
        _reportRepo.Setup(r => r.HasUserReportedContentAsync("u1", postId, ReportContentType.BlogPost)).ReturnsAsync(false);
        _blogRepo.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new BlogPost { Id = postId, AuthorId = "author-1", Title = "T", Content = new string('x', 10) });
        _reportRepo.Setup(r => r.CreateAsync(It.IsAny<Report>())).ReturnsAsync((Report rep) => { rep.Id = Guid.NewGuid(); return rep; });

    var create = new CreateReportDto { ContentId = postId, ContentType = ReportContentType.BlogPost, Reason = ReportReason.Spam, Description = "desc" };
        var dto = await svc.CreateReportAsync(create, "u1", "Reporter");
        dto.ContentId.Should().Be(postId);
        dto.Status.Should().Be(ReportStatus.Pending);
    }

    [Fact]
    public async Task CreateReportAsync_Duplicate_Throws()
    {
        var svc = CreateService();
        var postId = Guid.NewGuid();
        _reportRepo.Setup(r => r.HasUserReportedContentAsync("u1", postId, ReportContentType.BlogPost)).ReturnsAsync(true);
    var create = new CreateReportDto { ContentId = postId, ContentType = ReportContentType.BlogPost, Reason = ReportReason.Spam };
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateReportAsync(create, "u1", "Reporter"));
    }

    [Fact]
    public async Task CreateReportAsync_SelfReport_Throws()
    {
        var svc = CreateService();
        var postId = Guid.NewGuid();
        _reportRepo.Setup(r => r.HasUserReportedContentAsync("u1", postId, ReportContentType.BlogPost)).ReturnsAsync(false);
        _blogRepo.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(new BlogPost { Id = postId, AuthorId = "u1", Title = "T", Content = "C" });
    var create = new CreateReportDto { ContentId = postId, ContentType = ReportContentType.BlogPost, Reason = ReportReason.Spam };
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateReportAsync(create, "u1", "Reporter"));
    }

    [Fact]
    public async Task ValidateReportAsync_BlogPostMissing_ReturnsFalse()
    {
        var svc = CreateService();
        var postId = Guid.NewGuid();
        var (can, msg) = await svc.ValidateReportAsync("u1", postId, ReportContentType.BlogPost);
        can.Should().BeFalse();
        msg.Should().Contain("does not exist");
    }

    [Fact]
    public async Task UpdateReportAsync_SetsReviewedFields()
    {
        var svc = CreateService();
        var id = Guid.NewGuid();
        var existing = new Report { Id = id, Status = ReportStatus.Pending, ContentId = Guid.NewGuid(), ContentType = ReportContentType.BlogPost };
        _reportRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _reportRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);
        var dto = await svc.UpdateReportAsync(id, new UpdateReportDto { Status = ReportStatus.Resolved, AdminNotes = "ok", UserBanned = false, ContentRemoved = false });
        dto.Status.Should().Be(ReportStatus.Resolved);
    }

    [Fact]
    public async Task GetReportStatsAsync_CachesResult()
    {
        var svc = CreateService();
        _reportRepo.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(10);
        _reportRepo.Setup(r => r.GetPendingCountAsync()).ReturnsAsync(2);
        _reportRepo.Setup(r => r.GetByStatusAsync(ReportStatus.Resolved, 1, int.MaxValue)).ReturnsAsync(new List<Report>{ new() });
        _reportRepo.Setup(r => r.GetByStatusAsync(ReportStatus.Dismissed, 1, int.MaxValue)).ReturnsAsync(new List<Report>{ });
        var first = await svc.GetReportStatsAsync();
        var second = await svc.GetReportStatsAsync();
        first.TotalReports.Should().Be(10);
        // repository GetTotalCountAsync should be called only once due to caching
        _reportRepo.Verify(r => r.GetTotalCountAsync(), Times.Once);
    }
}
