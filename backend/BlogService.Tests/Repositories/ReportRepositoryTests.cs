using BlogService.Models.Entities;
using BlogService.Repositories.Implementations;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Repositories;

public class ReportRepositoryTests : IDisposable
{
    private readonly BlogDbContext _context;
    private readonly ReportRepository _repository;

    public ReportRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new ReportRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<Report> CreateTestReportAsync(
        ReportStatus status = ReportStatus.Pending,
        string reporterId = "reporter1",
        string reportedUserId = "reported1",
        ReportContentType contentType = ReportContentType.BlogPost)
    {
        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = reporterId,
            ReporterName = "Reporter Name",
            ReportedUserId = reportedUserId,
            ReportedUserName = "Reported User Name",
            ContentId = Guid.NewGuid(),
            ContentType = contentType,
            Reason = "Test reason",
            Description = "Test description",
            Status = status
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingReport_ReturnsReport()
    {
        var report = await CreateTestReportAsync();

        var result = await _repository.GetByIdAsync(report.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(report.Id);
        result.ReporterId.Should().Be("reporter1");
        result.Status.Should().Be(ReportStatus.Pending);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentReport_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleReports_ReturnsPagedResultsOrderedByDate()
    {
        await Task.Delay(10);
        var report1 = await CreateTestReportAsync();
        
        await Task.Delay(10);
        var report2 = await CreateTestReportAsync();
        
        await Task.Delay(10);
        var report3 = await CreateTestReportAsync();

        var result = await _repository.GetAllAsync(1, 2); // First page with 2 items

        var reports = result.ToList();
        reports.Should().HaveCount(2);
        
        // Should be ordered by CreatedAt descending (newest first)
        reports[0].CreatedAt.Should().BeAfter(reports[1].CreatedAt);
    }

    [Fact]
    public async Task GetAllAsync_SecondPage_ReturnsCorrectResults()
    {
        var report1 = await CreateTestReportAsync();
        var report2 = await CreateTestReportAsync();
        var report3 = await CreateTestReportAsync();

        var result = await _repository.GetAllAsync(2, 2); // Second page with 2 items

        var reports = result.ToList();
        reports.Should().HaveCount(1); // Only one item should be on second page
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmpty()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByStatusAsync_FiltersByStatus_ReturnsCorrectReports()
    {
        await CreateTestReportAsync(ReportStatus.Pending);
        await CreateTestReportAsync(ReportStatus.Resolved);
        await CreateTestReportAsync(ReportStatus.Pending);
        await CreateTestReportAsync(ReportStatus.Dismissed);

        var result = await _repository.GetByStatusAsync(ReportStatus.Pending);

        var reports = result.ToList();
        reports.Should().HaveCount(2);
        reports.All(r => r.Status == ReportStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task GetByStatusAsync_NoMatchingStatus_ReturnsEmpty()
    {
        await CreateTestReportAsync(ReportStatus.Pending);
        await CreateTestReportAsync(ReportStatus.Resolved);

        var result = await _repository.GetByStatusAsync(ReportStatus.Dismissed);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByReporterIdAsync_FiltersByReporter_ReturnsCorrectReports()
    {
        await CreateTestReportAsync(reporterId: "reporter1");
        await CreateTestReportAsync(reporterId: "reporter2");
        await CreateTestReportAsync(reporterId: "reporter1");

        var result = await _repository.GetByReporterIdAsync("reporter1");

        var reports = result.ToList();
        reports.Should().HaveCount(2);
        reports.All(r => r.ReporterId == "reporter1").Should().BeTrue();
    }

    [Fact]
    public async Task GetByReporterIdAsync_NoMatchingReporter_ReturnsEmpty()
    {
        await CreateTestReportAsync(reporterId: "reporter1");

        var result = await _repository.GetByReporterIdAsync("nonexistent-reporter");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByReportedUserIdAsync_FiltersByReportedUser_ReturnsCorrectReports()
    {
        await CreateTestReportAsync(reportedUserId: "user1");
        await CreateTestReportAsync(reportedUserId: "user2");
        await CreateTestReportAsync(reportedUserId: "user1");

        var result = await _repository.GetByReportedUserIdAsync("user1");

        var reports = result.ToList();
        reports.Should().HaveCount(2);
        reports.All(r => r.ReportedUserId == "user1").Should().BeTrue();
    }

    [Fact]
    public async Task GetByContentIdAsync_FiltersByContentIdAndType_ReturnsCorrectReports()
    {
        var contentId = Guid.NewGuid();
        
        var report1 = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = "reporter1",
            ReporterName = "Reporter",
            ReportedUserId = "reported1",
            ContentId = contentId,
            ContentType = ReportContentType.BlogPost,
            Reason = "Spam",
            Status = ReportStatus.Pending
        };
        
        var report2 = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = "reporter2",
            ReporterName = "Reporter 2",
            ReportedUserId = "reported2",
            ContentId = contentId,
            ContentType = ReportContentType.Comment, // Different content type
            Reason = "Harassment",
            Status = ReportStatus.Pending
        };

        _context.Reports.AddRange(report1, report2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByContentIdAsync(contentId, ReportContentType.BlogPost);

        var reports = result.ToList();
        reports.Should().HaveCount(1);
        reports[0].ContentType.Should().Be(ReportContentType.BlogPost);
    }

    [Fact]
    public async Task CreateAsync_ValidReport_CreatesAndReturnsReport()
    {
        var report = new Report
        {
            ReporterId = "new-reporter",
            ReporterName = "New Reporter",
            ReportedUserId = "new-reported",
            ReportedUserName = "New Reported User",
            ContentId = Guid.NewGuid(),
            ContentType = ReportContentType.Comment,
            Reason = "Inappropriate content",
            Description = "This content is inappropriate",
            Status = ReportStatus.Pending
        };

        var result = await _repository.CreateAsync(report);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.ReporterId.Should().Be("new-reporter");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var dbReport = await _context.Reports.FindAsync(result.Id);
        dbReport.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingReport_UpdatesReport()
    {
        var report = await CreateTestReportAsync();
        var originalCreatedAt = report.CreatedAt;

        report.Status = ReportStatus.Resolved;
        report.AdminNotes = "Resolved by admin";
        
        var result = await _repository.UpdateAsync(report);

        result.Should().NotBeNull();
        result.Status.Should().Be(ReportStatus.Resolved);
        result.AdminNotes.Should().Be("Resolved by admin");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CreatedAt.Should().Be(originalCreatedAt); // Should not change

        // Verify in database
        var dbReport = await _context.Reports.FindAsync(report.Id);
        dbReport!.Status.Should().Be(ReportStatus.Resolved);
        dbReport.AdminNotes.Should().Be("Resolved by admin");
    }

    [Fact]
    public async Task DeleteAsync_ExistingReport_RemovesReportAndReturnsTrue()
    {
        var report = await CreateTestReportAsync();

        var result = await _repository.DeleteAsync(report.Id);

        result.Should().BeTrue();

        var dbReport = await _context.Reports.FindAsync(report.Id);
        dbReport.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentReport_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTotalCountAsync_MultipleReports_ReturnsCorrectCount()
    {
        await CreateTestReportAsync();
        await CreateTestReportAsync();
        await CreateTestReportAsync();

        var result = await _repository.GetTotalCountAsync();

        result.Should().Be(3);
    }

    [Fact]
    public async Task GetTotalCountAsync_NoReports_ReturnsZero()
    {
        var result = await _repository.GetTotalCountAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetPendingCountAsync_MixedStatuses_ReturnsOnlyPendingCount()
    {
        await CreateTestReportAsync(ReportStatus.Pending);
        await CreateTestReportAsync(ReportStatus.Resolved);
        await CreateTestReportAsync(ReportStatus.Pending);
        await CreateTestReportAsync(ReportStatus.Dismissed);
        await CreateTestReportAsync(ReportStatus.Pending);

        var result = await _repository.GetPendingCountAsync();

        result.Should().Be(3);
    }

    [Fact]
    public async Task GetPendingCountAsync_NoPendingReports_ReturnsZero()
    {
        await CreateTestReportAsync(ReportStatus.Resolved);
        await CreateTestReportAsync(ReportStatus.Dismissed);

        var result = await _repository.GetPendingCountAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task HasUserReportedContentAsync_UserHasReported_ReturnsTrue()
    {
        var contentId = Guid.NewGuid();
        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = "test-user",
            ReporterName = "Test User",
            ReportedUserId = "reported-user",
            ContentId = contentId,
            ContentType = ReportContentType.BlogPost,
            Reason = "Spam",
            Status = ReportStatus.Pending
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var result = await _repository.HasUserReportedContentAsync("test-user", contentId, ReportContentType.BlogPost);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasUserReportedContentAsync_UserHasNotReported_ReturnsFalse()
    {
        var contentId = Guid.NewGuid();

        var result = await _repository.HasUserReportedContentAsync("test-user", contentId, ReportContentType.BlogPost);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasUserReportedContentAsync_DifferentContentType_ReturnsFalse()
    {
        var contentId = Guid.NewGuid();
        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = "test-user",
            ReporterName = "Test User",
            ReportedUserId = "reported-user",
            ContentId = contentId,
            ContentType = ReportContentType.BlogPost, // Reported as blog post
            Reason = "Spam",
            Status = ReportStatus.Pending
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Check for comment type - should return false
        var result = await _repository.HasUserReportedContentAsync("test-user", contentId, ReportContentType.Comment);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteReportLifecycle_CreateUpdateDelete_WorksCorrectly()
    {
        // Create report
        var report = new Report
        {
            ReporterId = "lifecycle-reporter",
            ReporterName = "Lifecycle Reporter",
            ReportedUserId = "lifecycle-reported",
            ReportedUserName = "Lifecycle Reported",
            ContentId = Guid.NewGuid(),
            ContentType = ReportContentType.BlogPost,
            Reason = "Test lifecycle",
            Status = ReportStatus.Pending
        };
        var created = await _repository.CreateAsync(report);
        
        // Verify creation
        var totalCount1 = await _repository.GetTotalCountAsync();
        var pendingCount1 = await _repository.GetPendingCountAsync();
        totalCount1.Should().Be(1);
        pendingCount1.Should().Be(1);
        
        // Update report
        created.Status = ReportStatus.Resolved;
        created.AdminNotes = "Issue resolved";
        await _repository.UpdateAsync(created);
        
        // Verify update
        var pendingCount2 = await _repository.GetPendingCountAsync();
        pendingCount2.Should().Be(0); // No longer pending
        
        var resolvedReports = await _repository.GetByStatusAsync(ReportStatus.Resolved);
        resolvedReports.Should().HaveCount(1);
        
        // Delete report
        var deleteResult = await _repository.DeleteAsync(created.Id);
        deleteResult.Should().BeTrue();
        
        // Verify deletion
        var totalCount3 = await _repository.GetTotalCountAsync();
        totalCount3.Should().Be(0);
        
        var exists = await _repository.GetByIdAsync(created.Id);
        exists.Should().BeNull();
    }
}
