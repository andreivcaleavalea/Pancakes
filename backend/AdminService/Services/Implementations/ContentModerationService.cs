using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Models.Entities;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AdminService.Clients;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Services.Implementations
{
    public class ContentModerationService : IContentModerationService
    {
        private readonly AdminDbContext _context;
        private readonly BlogServiceClient _blogServiceClient;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<ContentModerationService> _logger;

        public ContentModerationService(
            AdminDbContext context,
            BlogServiceClient blogServiceClient,
            IAuditService auditService,
            IMapper mapper,
            ILogger<ContentModerationService> logger)
        {
            _context = context;
            _blogServiceClient = blogServiceClient;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<BlogPostModerationDto>> SearchBlogPostsAsync(ContentSearchRequest request)
        {
            var posts = await _blogServiceClient.SearchBlogPostsAsync(request.SearchTerm, request.Page, request.PageSize);
            
            return new PagedResponse<BlogPostModerationDto>
            {
                Data = posts,
                TotalCount = posts.Count,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)posts.Count / request.PageSize),
                HasNext = request.Page * request.PageSize < posts.Count,
                HasPrevious = request.Page > 1
            };
        }

        public async Task<BlogPostModerationDto?> GetBlogPostDetailsAsync(Guid blogPostId)
        {
            return await _blogServiceClient.GetBlogPostAsync(blogPostId);
        }

        public async Task<bool> ModerateBlogPostAsync(ModerateBlogPostRequest request, string moderatedBy)
        {
            var success = await _blogServiceClient.ModerateBlogPostAsync(request.BlogPostId, request.Action, request.Reason);
            
            if (success)
            {
                await _auditService.LogActionAsync(moderatedBy, $"BLOG_POST_{request.Action.ToUpper()}", "BlogPost", 
                    request.BlogPostId.ToString(), new { request.Reason, request.NotifyAuthor }, "", "");
            }

            return success;
        }

        public async Task<bool> BulkModerateBlogPostsAsync(BulkModerationRequest request, string moderatedBy)
        {
            var successCount = 0;
            foreach (var contentId in request.ContentIds)
            {
                if (Guid.TryParse(contentId, out var blogPostId))
                {
                    var success = await _blogServiceClient.ModerateBlogPostAsync(blogPostId, request.Action, request.Reason);
                    if (success) successCount++;
                }
            }

            await _auditService.LogActionAsync(moderatedBy, $"BULK_BLOG_POST_{request.Action.ToUpper()}", "BlogPost", 
                string.Join(",", request.ContentIds), new { request.Reason, SuccessCount = successCount }, "", "");

            return successCount > 0;
        }

        public async Task<PagedResponse<CommentModerationDto>> SearchCommentsAsync(ContentSearchRequest request)
        {
            var comments = await _blogServiceClient.SearchCommentsAsync(request.SearchTerm, request.Page, request.PageSize);
            
            return new PagedResponse<CommentModerationDto>
            {
                Data = comments,
                TotalCount = comments.Count,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)comments.Count / request.PageSize),
                HasNext = request.Page * request.PageSize < comments.Count,
                HasPrevious = request.Page > 1
            };
        }

        public async Task<CommentModerationDto?> GetCommentDetailsAsync(Guid commentId)
        {
            return await _blogServiceClient.GetCommentAsync(commentId);
        }

        public async Task<bool> ModerateCommentAsync(ModerateCommentRequest request, string moderatedBy)
        {
            var success = await _blogServiceClient.ModerateCommentAsync(request.CommentId, request.Action, request.Reason);
            
            if (success)
            {
                await _auditService.LogActionAsync(moderatedBy, $"COMMENT_{request.Action.ToUpper()}", "Comment", 
                    request.CommentId.ToString(), new { request.Reason, request.NotifyAuthor }, "", "");
            }

            return success;
        }

        public async Task<bool> BulkModerateCommentsAsync(BulkModerationRequest request, string moderatedBy)
        {
            var successCount = 0;
            foreach (var contentId in request.ContentIds)
            {
                if (Guid.TryParse(contentId, out var commentId))
                {
                    var success = await _blogServiceClient.ModerateCommentAsync(commentId, request.Action, request.Reason);
                    if (success) successCount++;
                }
            }

            await _auditService.LogActionAsync(moderatedBy, $"BULK_COMMENT_{request.Action.ToUpper()}", "Comment", 
                string.Join(",", request.ContentIds), new { request.Reason, SuccessCount = successCount }, "", "");

            return successCount > 0;
        }

        public async Task<ContentFlagDto> CreateContentFlagAsync(CreateContentFlagRequest request, string? flaggedBy)
        {
            var flag = new ContentFlag
            {
                ContentType = request.ContentType,
                ContentId = request.ContentId,
                FlagType = request.FlagType,
                FlaggedBy = flaggedBy,
                AutoDetected = request.AutoDetected,
                Severity = request.Severity,
                Description = request.Description
            };

            _context.ContentFlags.Add(flag);
            await _context.SaveChangesAsync();

            return _mapper.Map<ContentFlagDto>(flag);
        }

        public async Task<PagedResponse<ContentFlagDto>> GetContentFlagsAsync(int page, int pageSize, string? status = null, string? contentType = null)
        {
            var query = _context.ContentFlags.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(f => f.Status == status);

            if (!string.IsNullOrEmpty(contentType))
                query = query.Where(f => f.ContentType == contentType);

            var totalCount = await query.CountAsync();

            var flags = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var flagDtos = _mapper.Map<List<ContentFlagDto>>(flags);

            return new PagedResponse<ContentFlagDto>
            {
                Data = flagDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNext = page * pageSize < totalCount,
                HasPrevious = page > 1
            };
        }

        public async Task<bool> ReviewContentFlagAsync(ReviewFlagRequest request, string reviewedBy)
        {
            var flag = await _context.ContentFlags.FindAsync(request.FlagId);
            if (flag == null) return false;

            flag.Status = request.Action;
            flag.ReviewedBy = reviewedBy;
            flag.ReviewedAt = DateTime.UtcNow;
            flag.ReviewNotes = request.ReviewNotes;
            flag.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(reviewedBy, $"FLAG_{request.Action.ToUpper()}", "ContentFlag", 
                request.FlagId.ToString(), new { request.ReviewNotes }, "", "");

            return true;
        }

        public async Task<List<ContentFlagDto>> GetPendingFlagsAsync(int count = 20)
        {
            var flags = await _context.ContentFlags
                .Where(f => f.Status == "pending")
                .OrderByDescending(f => f.Severity)
                .ThenByDescending(f => f.CreatedAt)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<List<ContentFlagDto>>(flags);
        }

        public async Task<UserReportDto> CreateUserReportAsync(CreateUserReportRequest request)
        {
            var report = new UserReport
            {
                ReportedUserId = request.ReportedUserId,
                ReporterUserId = request.ReporterUserId,
                Reason = request.Reason,
                Description = request.Description
            };

            _context.UserReports.Add(report);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserReportDto>(report);
        }

        public async Task<PagedResponse<UserReportDto>> GetUserReportsAsync(int page, int pageSize, string? status = null)
        {
            var query = _context.UserReports.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            var totalCount = await query.CountAsync();

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reportDtos = _mapper.Map<List<UserReportDto>>(reports);

            return new PagedResponse<UserReportDto>
            {
                Data = reportDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNext = page * pageSize < totalCount,
                HasPrevious = page > 1
            };
        }

        public async Task<bool> ReviewUserReportAsync(ReviewUserReportRequest request, string reviewedBy)
        {
            var report = await _context.UserReports.FindAsync(request.ReportId);
            if (report == null) return false;

            report.Status = request.Action;
            report.ReviewedBy = reviewedBy;
            report.ReviewedAt = DateTime.UtcNow;
            report.ReviewNotes = request.ReviewNotes;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(reviewedBy, $"REPORT_{request.Action.ToUpper()}", "UserReport", 
                request.ReportId.ToString(), new { request.ReviewNotes }, "", "");

            return true;
        }

        public async Task<List<UserReportDto>> GetPendingReportsAsync(int count = 20)
        {
            var reports = await _context.UserReports
                .Where(r => r.Status == "pending")
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<List<UserReportDto>>(reports);
        }

        public async Task<Dictionary<string, object>> GetModerationStatisticsAsync()
        {
            var totalReports = await _context.UserReports.CountAsync();
            var pendingReports = await _context.UserReports.CountAsync(r => r.Status == "pending");
            var totalFlags = await _context.ContentFlags.CountAsync();
            var pendingFlags = await _context.ContentFlags.CountAsync(f => f.Status == "pending");

            return new Dictionary<string, object>
            {
                { "totalReports", totalReports },
                { "pendingReports", pendingReports },
                { "totalFlags", totalFlags },
                { "pendingFlags", pendingFlags }
            };
        }

        public async Task<List<ContentFlagDto>> GetHighSeverityFlagsAsync()
        {
            var flags = await _context.ContentFlags
                .Where(f => f.Severity >= 4 && f.Status == "pending")
                .OrderByDescending(f => f.Severity)
                .ThenByDescending(f => f.CreatedAt)
                .Take(10)
                .ToListAsync();

            return _mapper.Map<List<ContentFlagDto>>(flags);
        }

        public async Task<bool> AutoModerationScanAsync()
        {
            // Would implement auto-moderation logic here
            _logger.LogInformation("Auto-moderation scan completed");
            return true;
        }
    }
}