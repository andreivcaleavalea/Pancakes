using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Clients.BlogClient.DTOs;
using AdminService.Clients.BlogClient.Services;
using AdminService.Services.Interfaces;

namespace AdminService.Services.Implementations
{
    public class BlogManagementService : IBlogManagementService
    {
        private readonly IBlogServiceClient _blogServiceClient;
        private readonly IAuditService _auditService;
        private readonly ILogger<BlogManagementService> _logger;

        public BlogManagementService(
            IBlogServiceClient blogServiceClient,
            IAuditService auditService,
            ILogger<BlogManagementService> logger)
        {
            _blogServiceClient = blogServiceClient;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ServiceResult<PagedResponse<BlogPostDTO>>> SearchBlogPostsAsync(BlogPostSearchRequest request)
        {
            try
            {
                var blogPosts = await _blogServiceClient.SearchBlogPostsAsync(request);
                return ServiceResult<PagedResponse<BlogPostDTO>>.SuccessResult(blogPosts, "Blog posts retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching blog posts");
                return ServiceResult<PagedResponse<BlogPostDTO>>.FailureResult(
                    "An error occurred while searching blog posts", ex.Message);
            }
        }

        public async Task<ServiceResult<string>> DeleteBlogPostAsync(string blogPostId, DeleteBlogPostRequest request, string adminId, string ipAddress, string userAgent)
        {
            try
            {
                var success = await _blogServiceClient.DeleteBlogPostAsync(blogPostId, adminId);
                
                if (success)
                {
                    await _auditService.LogActionAsync(adminId, "DELETE_BLOG_POST", "BlogPost", blogPostId,
                        new { Reason = request.Reason }, ipAddress, userAgent);

                    return ServiceResult<string>.SuccessResult("Blog post deleted successfully");
                }

                return ServiceResult<string>.FailureResult("Failed to delete blog post");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog post {BlogPostId}", blogPostId);
                return ServiceResult<string>.FailureResult(
                    "An error occurred while deleting the blog post", ex.Message);
            }
        }

        public async Task<ServiceResult<string>> UpdateBlogPostStatusAsync(string blogPostId, UpdateBlogPostStatusRequest request, string adminId, string ipAddress, string userAgent)
        {
            try
            {
                var success = await _blogServiceClient.UpdateBlogPostStatusAsync(blogPostId, request.Status, adminId);
                
                if (success)
                {
                    await _auditService.LogActionAsync(adminId, "UPDATE_BLOG_POST_STATUS", "BlogPost", blogPostId,
                        new { StatusChanged = request.Status, Reason = request.Reason }, ipAddress, userAgent);

                    var statusName = GetStatusName(request.Status);
                    return ServiceResult<string>.SuccessResult($"Blog post status updated to {statusName} successfully");
                }

                return ServiceResult<string>.FailureResult("Failed to update blog post status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog post status {BlogPostId}", blogPostId);
                return ServiceResult<string>.FailureResult(
                    "An error occurred while updating the blog post status", ex.Message);
            }
        }

        public async Task<ServiceResult<Dictionary<string, object>>> GetBlogStatisticsAsync()
        {
            try
            {
                var stats = await _blogServiceClient.GetBlogStatisticsAsync();
                return ServiceResult<Dictionary<string, object>>.SuccessResult(stats, "Blog statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog statistics");
                return ServiceResult<Dictionary<string, object>>.FailureResult(
                    "An error occurred while retrieving blog statistics", ex.Message);
            }
        }

        private static string GetStatusName(int status)
        {
            return status switch
            {
                0 => "Draft",
                1 => "Published", 
                2 => "Deleted",
                _ => "Unknown"
            };
        }
    }
}
