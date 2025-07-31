using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;

namespace AdminService.Services.Interfaces
{
    public interface IContentModerationService
    {
        // Blog Post Moderation
        Task<PagedResponse<BlogPostModerationDto>> SearchBlogPostsAsync(ContentSearchRequest request);
        Task<BlogPostModerationDto?> GetBlogPostDetailsAsync(Guid blogPostId);
        Task<bool> ModerateBlogPostAsync(ModerateBlogPostRequest request, string moderatedBy);
        Task<bool> BulkModerateBlogPostsAsync(BulkModerationRequest request, string moderatedBy);
        
        // Comment Moderation
        Task<PagedResponse<CommentModerationDto>> SearchCommentsAsync(ContentSearchRequest request);
        Task<CommentModerationDto?> GetCommentDetailsAsync(Guid commentId);
        Task<bool> ModerateCommentAsync(ModerateCommentRequest request, string moderatedBy);
        Task<bool> BulkModerateCommentsAsync(BulkModerationRequest request, string moderatedBy);
        
        // Content Flags
        Task<ContentFlagDto> CreateContentFlagAsync(CreateContentFlagRequest request, string? flaggedBy);
        Task<PagedResponse<ContentFlagDto>> GetContentFlagsAsync(int page, int pageSize, string? status = null, string? contentType = null);
        Task<bool> ReviewContentFlagAsync(ReviewFlagRequest request, string reviewedBy);
        Task<List<ContentFlagDto>> GetPendingFlagsAsync(int count = 20);
        
        // User Reports
        Task<UserReportDto> CreateUserReportAsync(CreateUserReportRequest request);
        Task<PagedResponse<UserReportDto>> GetUserReportsAsync(int page, int pageSize, string? status = null);
        Task<bool> ReviewUserReportAsync(ReviewUserReportRequest request, string reviewedBy);
        Task<List<UserReportDto>> GetPendingReportsAsync(int count = 20);
        
        // Statistics
        Task<Dictionary<string, object>> GetModerationStatisticsAsync();
        Task<List<ContentFlagDto>> GetHighSeverityFlagsAsync();
        Task<bool> AutoModerationScanAsync();
    }
}