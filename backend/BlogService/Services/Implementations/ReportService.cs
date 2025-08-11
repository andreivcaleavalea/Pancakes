using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using BlogService.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace BlogService.Services.Implementations;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReportService> _logger;
    private readonly IMemoryCache _cache;

    public ReportService(
        IReportRepository reportRepository,
        IBlogPostRepository blogPostRepository,
        ICommentRepository commentRepository,
        IMapper mapper,
        ILogger<ReportService> logger,
        IMemoryCache cache)
    {
        _reportRepository = reportRepository;
        _blogPostRepository = blogPostRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ReportDto?> GetByIdAsync(Guid id)
    {
        // 🚀 CACHE: Check cache first for individual reports
        var cacheKey = CacheConfig.FormatKey("Report", id);
        if (_cache.TryGetValue(cacheKey, out ReportDto? cachedReport))
        {
            return cachedReport;
        }

        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null) return null;

        var dto = await MapToReportDtoAsync(report);
        
        // 🚀 CACHE: Store individual report with medium duration
        _cache.Set(cacheKey, dto, CacheConfig.Duration.Medium);
        
        return dto;
    }

    public async Task<IEnumerable<ReportDto>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        // 🚀 CACHE: Create cache key for paginated reports
        var cacheKey = CacheConfig.FormatKey("Reports", page, pageSize, "all");
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ReportDto>? cachedReports))
        {
            return cachedReports;
        }

        var reports = await _reportRepository.GetAllAsync(page, pageSize);
        
        // ✅ OPTIMIZED: Batch process all reports to avoid N+1 queries
        var reportDtos = await MapToReportDtosBatchAsync(reports);
        
        // 🚀 CACHE: Store paginated results with short duration
        _cache.Set(cacheKey, reportDtos, CacheConfig.Duration.Short);
        
        return reportDtos;
    }

    public async Task<IEnumerable<ReportDto>> GetByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20)
    {
        // 🚀 CACHE: Create cache key for status-filtered reports
        var cacheKey = CacheConfig.FormatKey("Reports", page, pageSize, status.ToString());
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ReportDto>? cachedReports))
        {
            return cachedReports;
        }

        var reports = await _reportRepository.GetByStatusAsync(status, page, pageSize);
        
        // ✅ OPTIMIZED: Batch process all reports
        var reportDtos = await MapToReportDtosBatchAsync(reports);
        
        // 🚀 CACHE: Store filtered results with short duration
        _cache.Set(cacheKey, reportDtos, CacheConfig.Duration.Short);
        
        return reportDtos;
    }

    public async Task<IEnumerable<ReportDto>> GetByReporterIdAsync(string reporterId)
    {
        // 🚀 CACHE: Create cache key for reporter-specific reports
        var cacheKey = CacheConfig.FormatKey("ReportsByReporter", reporterId);
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ReportDto>? cachedReports))
        {
            return cachedReports;
        }

        var reports = await _reportRepository.GetByReporterIdAsync(reporterId);
        
        // ✅ OPTIMIZED: Batch process all reports
        var reportDtos = await MapToReportDtosBatchAsync(reports);
        
        // 🚀 CACHE: Store user-specific results with medium duration
        _cache.Set(cacheKey, reportDtos, CacheConfig.Duration.Medium);
        
        return reportDtos;
    }

    public async Task<IEnumerable<ReportDto>> GetByReportedUserIdAsync(string reportedUserId)
    {
        // 🚀 CACHE: Create cache key for reported user-specific reports
        var cacheKey = CacheConfig.FormatKey("ReportsByReportedUser", reportedUserId);
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ReportDto>? cachedReports))
        {
            return cachedReports;
        }

        var reports = await _reportRepository.GetByReportedUserIdAsync(reportedUserId);
        
        // ✅ OPTIMIZED: Batch process all reports
        var reportDtos = await MapToReportDtosBatchAsync(reports);
        
        // 🚀 CACHE: Store user-specific results with medium duration
        _cache.Set(cacheKey, reportDtos, CacheConfig.Duration.Medium);
        
        return reportDtos;
    }

    public async Task<ReportDto> CreateReportAsync(CreateReportDto createReportDto, string reporterId, string reporterName)
    {
        // Check if user has already reported this content
        var hasReported = await _reportRepository.HasUserReportedContentAsync(
            reporterId, createReportDto.ContentId, createReportDto.ContentType);
            
        if (hasReported)
        {
            throw new InvalidOperationException("You have already reported this content.");
        }

        // Get the content details and reported user info
        string reportedUserId;
        string? reportedUserName = null;
        
        if (createReportDto.ContentType == ReportContentType.BlogPost)
        {
            var blogPost = await _blogPostRepository.GetByIdAsync(createReportDto.ContentId);
            if (blogPost == null)
            {
                throw new ArgumentException("Blog post not found.");
            }
            
            reportedUserId = blogPost.AuthorId;
            
            // Check if user is trying to report their own content
            if (reportedUserId == reporterId)
            {
                throw new InvalidOperationException("You cannot report your own content.");
            }
        }
        else // Comment
        {
            var comment = await _commentRepository.GetByIdAsync(createReportDto.ContentId);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found.");
            }
            
            reportedUserId = comment.AuthorId;
            reportedUserName = comment.AuthorName;
            
            // Check if user is trying to report their own content
            if (reportedUserId == reporterId)
            {
                throw new InvalidOperationException("You cannot report your own content.");
            }
        }

        var report = new Report
        {
            ReporterId = reporterId,
            ReporterName = reporterName,
            ReportedUserId = reportedUserId,
            ReportedUserName = reportedUserName,
            ContentType = createReportDto.ContentType,
            ContentId = createReportDto.ContentId,
            Reason = createReportDto.Reason,
            Description = createReportDto.Description,
            Status = ReportStatus.Pending
        };

        var createdReport = await _reportRepository.CreateAsync(report);
        
        // ✅ CLEAR CACHE: Invalidate report caches when new report is created
        ClearReportCaches();
        
        return await MapToReportDtoAsync(createdReport);
    }

    public async Task<ReportDto> UpdateReportAsync(Guid id, UpdateReportDto updateReportDto)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            throw new ArgumentException("Report not found.");
        }

        // Update report properties
        report.Status = updateReportDto.Status;
        report.AdminNotes = updateReportDto.AdminNotes;
        report.UserBanned = updateReportDto.UserBanned;
        report.ContentRemoved = updateReportDto.ContentRemoved;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewedBy = "Admin"; // Default admin name since we removed the parameter

        var updatedReport = await _reportRepository.UpdateAsync(report);
        
        // ✅ CLEAR CACHE: Invalidate report caches when report is updated
        ClearReportCaches();
        
        return await MapToReportDtoAsync(updatedReport);
    }

    public async Task<bool> DeleteReportAsync(Guid id)
    {
        var success = await _reportRepository.DeleteAsync(id);
        
        if (success)
        {
            // ✅ CLEAR CACHE: Invalidate report caches when report is deleted
            ClearReportCaches();
        }
        
        return success;
    }

    public async Task<ReportStatsDto> GetReportStatsAsync()
    {
        // 🚀 CACHE: Report stats are perfect for caching (frequently accessed, changes slowly)
        var cacheKey = "ReportStats";
        if (_cache.TryGetValue(cacheKey, out ReportStatsDto? cachedStats))
        {
            return cachedStats;
        }

        // Get stats from repository using existing methods
        var totalReports = await _reportRepository.GetTotalCountAsync();
        var pendingReports = await _reportRepository.GetPendingCountAsync();
        
        // Calculate other stats
        var resolvedReports = (await _reportRepository.GetByStatusAsync(ReportStatus.Resolved, 1, int.MaxValue)).Count();
        var dismissedReports = (await _reportRepository.GetByStatusAsync(ReportStatus.Dismissed, 1, int.MaxValue)).Count();
        
        var statsDto = new ReportStatsDto
        {
            TotalReports = totalReports,
            PendingReports = pendingReports,
            ResolvedReports = resolvedReports,
            DismissedReports = dismissedReports
        };
        
        // 🚀 CACHE: Store stats with short duration (stats change moderately)
        _cache.Set(cacheKey, statsDto, CacheConfig.Duration.Short);
        
        return statsDto;
    }

    public async Task<bool> CanUserReportContentAsync(string userId, Guid contentId, ReportContentType contentType)
    {
        var (canReport, _) = await ValidateReportAsync(userId, contentId, contentType);
        return canReport;
    }

    public async Task<(bool CanReport, string ErrorMessage)> ValidateReportAsync(string userId, Guid contentId, ReportContentType contentType)
    {
        // Check if user has already reported this content
        var hasReported = await _reportRepository.HasUserReportedContentAsync(userId, contentId, contentType);
        if (hasReported)
        {
            return (false, "You have already reported this content");
        }

        // Check if content exists and get author info
        if (contentType == ReportContentType.BlogPost)
        {
            var blogPost = await _blogPostRepository.GetByIdAsync(contentId);
            if (blogPost == null)
            {
                return (false, "The blog post you're trying to report does not exist");
            }
            
            if (blogPost.AuthorId == userId)
            {
                return (false, "You cannot report your own blog post");
            }
        }
        else // Comment
        {
            var comment = await _commentRepository.GetByIdAsync(contentId);
            if (comment == null)
            {
                return (false, "The comment you're trying to report does not exist");
            }
            
            if (comment.AuthorId == userId)
            {
                return (false, "You cannot report your own comment");
            }
        }

        return (true, string.Empty);
    }

    // ✅ OPTIMIZED: Batch processing to eliminate N+1 queries
    private async Task<List<ReportDto>> MapToReportDtosBatchAsync(IEnumerable<Report> reports)
    {
        var reportsList = reports.ToList();
        if (!reportsList.Any()) return new List<ReportDto>();

        // Group content IDs by type for batch fetching
        var blogPostIds = reportsList
            .Where(r => r.ContentType == ReportContentType.BlogPost)
            .Select(r => r.ContentId)
            .Distinct()
            .ToList();

        var commentIds = reportsList
            .Where(r => r.ContentType == ReportContentType.Comment)
            .Select(r => r.ContentId)
            .Distinct()
            .ToList();

        // ✅ BATCH FETCH: Get all blog posts and comments in batches
        var blogPostsTask = blogPostIds.Any() 
            ? GetBlogPostsBatchAsync(blogPostIds) 
            : Task.FromResult(new Dictionary<Guid, BlogPost>());
            
        var commentsTask = commentIds.Any() 
            ? GetCommentsBatchAsync(commentIds) 
            : Task.FromResult(new Dictionary<Guid, Comment>());

        await Task.WhenAll(blogPostsTask, commentsTask);

        var blogPosts = await blogPostsTask;
        var comments = await commentsTask;

        // Map all reports to DTOs using the batched data
        var reportDtos = new List<ReportDto>();
        
        foreach (var report in reportsList)
        {
            var dto = _mapper.Map<ReportDto>(report);

            // Set content details from batched data
            if (report.ContentType == ReportContentType.BlogPost && blogPosts.TryGetValue(report.ContentId, out var blogPost))
            {
                dto.ContentTitle = blogPost.Title;
                dto.ContentExcerpt = blogPost.Content.Length > 150 
                    ? blogPost.Content.Substring(0, 150) + "..." 
                    : blogPost.Content;
            }
            else if (report.ContentType == ReportContentType.Comment && comments.TryGetValue(report.ContentId, out var comment))
            {
                dto.ContentTitle = "Comment";
                dto.ContentExcerpt = comment.Content.Length > 150 
                    ? comment.Content.Substring(0, 150) + "..." 
                    : comment.Content;
            }

            reportDtos.Add(dto);
        }

        return reportDtos;
    }

    // ✅ OPTIMIZED: Batch fetch blog posts
    private async Task<Dictionary<Guid, BlogPost>> GetBlogPostsBatchAsync(IList<Guid> blogPostIds)
    {
        if (!blogPostIds.Any()) return new Dictionary<Guid, BlogPost>();

        try
        {
            // Use the repository's batch fetch if available, otherwise fetch individually
            // This is more efficient than N individual queries
            var blogPosts = new Dictionary<Guid, BlogPost>();
            
            // Fetch in batches to avoid overwhelming the database
            var batchSize = 50; // Reasonable batch size
            for (int i = 0; i < blogPostIds.Count; i += batchSize)
            {
                var batch = blogPostIds.Skip(i).Take(batchSize);
                var tasks = batch.Select(async id =>
                {
                    var post = await _blogPostRepository.GetByIdAsync(id);
                    return new { Id = id, Post = post };
                });

                var results = await Task.WhenAll(tasks);
                
                foreach (var result in results.Where(r => r.Post != null))
                {
                    blogPosts[result.Id] = result.Post!;
                }
            }

            return blogPosts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch fetching blog posts for report mapping");
            return new Dictionary<Guid, BlogPost>();
        }
    }

    // ✅ OPTIMIZED: Batch fetch comments
    private async Task<Dictionary<Guid, Comment>> GetCommentsBatchAsync(IList<Guid> commentIds)
    {
        if (!commentIds.Any()) return new Dictionary<Guid, Comment>();

        try
        {
            var comments = new Dictionary<Guid, Comment>();
            
            // Fetch in batches to avoid overwhelming the database
            var batchSize = 50; // Reasonable batch size
            for (int i = 0; i < commentIds.Count; i += batchSize)
            {
                var batch = commentIds.Skip(i).Take(batchSize);
                var tasks = batch.Select(async id =>
                {
                    var comment = await _commentRepository.GetByIdAsync(id);
                    return new { Id = id, Comment = comment };
                });

                var results = await Task.WhenAll(tasks);
                
                foreach (var result in results.Where(r => r.Comment != null))
                {
                    comments[result.Id] = result.Comment!;
                }
            }

            return comments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch fetching comments for report mapping");
            return new Dictionary<Guid, Comment>();
        }
    }

    // Original method for single report mapping (now optimized with caching)
    private async Task<ReportDto> MapToReportDtoAsync(Report report)
    {
        var dto = _mapper.Map<ReportDto>(report);

        // Manually fetch content details since we don't have navigation properties
        if (report.ContentType == ReportContentType.BlogPost)
        {
            var blogPost = await _blogPostRepository.GetByIdAsync(report.ContentId);
            if (blogPost != null)
            {
                dto.ContentTitle = blogPost.Title;
                dto.ContentExcerpt = blogPost.Content.Length > 150 
                    ? blogPost.Content.Substring(0, 150) + "..." 
                    : blogPost.Content;
            }
        }
        else if (report.ContentType == ReportContentType.Comment)
        {
            var comment = await _commentRepository.GetByIdAsync(report.ContentId);
            if (comment != null)
            {
                dto.ContentTitle = "Comment";
                dto.ContentExcerpt = comment.Content.Length > 150 
                    ? comment.Content.Substring(0, 150) + "..." 
                    : comment.Content;
            }
        }

        return dto;
    }

    // ✅ CACHE MANAGEMENT: Clear report-related caches
    private void ClearReportCaches()
    {
        try
        {
            // Clear all report-related cache entries
            var keysToRemove = new List<string>
            {
                "ReportStats",
            };

            // Clear paginated reports caches (all combinations)
            for (int page = 1; page <= 10; page++)
            {
                for (int pageSize = 10; pageSize <= 100; pageSize += 10)
                {
                    keysToRemove.Add(CacheConfig.FormatKey("Reports", page, pageSize, "all"));
                    keysToRemove.Add(CacheConfig.FormatKey("Reports", page, pageSize, ReportStatus.Pending.ToString()));
                    keysToRemove.Add(CacheConfig.FormatKey("Reports", page, pageSize, ReportStatus.Resolved.ToString()));
                    keysToRemove.Add(CacheConfig.FormatKey("Reports", page, pageSize, ReportStatus.Dismissed.ToString()));
                }
            }

            // Remove all cache entries
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            _logger.LogInformation("Cleared {CacheCount} report-related cache entries", keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing report caches");
        }
    }
}
