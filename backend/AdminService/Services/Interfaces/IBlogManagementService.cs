using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Clients.BlogClient.DTOs;

namespace AdminService.Services.Interfaces
{
    public interface IBlogManagementService
    {
        Task<ServiceResult<PagedResponse<BlogPostDTO>>> SearchBlogPostsAsync(BlogPostSearchRequest request);
        Task<ServiceResult<string>> DeleteBlogPostAsync(string blogPostId, DeleteBlogPostRequest request, string adminId, string ipAddress, string userAgent);
        Task<ServiceResult<string>> UpdateBlogPostStatusAsync(string blogPostId, UpdateBlogPostStatusRequest request, string adminId, string ipAddress, string userAgent);
        Task<ServiceResult<Dictionary<string, object>>> GetBlogStatisticsAsync();
    }
}
