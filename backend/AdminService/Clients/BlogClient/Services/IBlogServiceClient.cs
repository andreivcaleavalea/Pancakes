using AdminService.Clients.BlogClient.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;

namespace AdminService.Clients.BlogClient.Services
{
    public interface IBlogServiceClient
    {
        Task<PagedResponse<BlogPostDTO>> SearchBlogPostsAsync(BlogPostSearchRequest request);
        Task<bool> DeleteBlogPostAsync(string blogPostId, string adminId);
        Task<bool> UpdateBlogPostStatusAsync(string blogPostId, int status, string adminId);
        Task<Dictionary<string, object>> GetBlogStatisticsAsync();
    }
}
