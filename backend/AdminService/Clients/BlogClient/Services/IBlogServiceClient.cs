using AdminService.Clients.BlogClient.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;

namespace AdminService.Clients.BlogClient.Services
{
    public interface IBlogServiceClient
    {
        Task<PagedResponse<BlogPostDTO>> SearchBlogPostsAsync(BlogPostSearchRequest request);
        Task<BlogPostDTO?> GetBlogPostByIdAsync(string blogPostId);
        Task<bool> DeleteBlogPostAsync(string blogPostId, string adminId);
        Task<bool> UpdateBlogPostStatusAsync(string blogPostId, int status, string adminId);
        Task<Dictionary<string, object>> GetBlogStatisticsAsync();
        
        // Report management methods
        Task<string> GetReportsAsync(int page = 1, int pageSize = 20, int? status = null);
        Task<string> GetReportByIdAsync(string reportId);
        Task<bool> UpdateReportAsync(string reportId, object updateData);
        Task<bool> DeleteReportAsync(string reportId);
        Task<string> GetReportStatsAsync();
    }
}
