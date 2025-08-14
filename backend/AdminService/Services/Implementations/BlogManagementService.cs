using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Clients.BlogClient.DTOs;
using AdminService.Clients.BlogClient.Services;
using AdminService.Clients.UserClient;
using AdminService.Services.Interfaces;

namespace AdminService.Services.Implementations
{
    public class BlogManagementService : IBlogManagementService
    {
        private readonly IBlogServiceClient _blogServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IAuditService _auditService;
        private readonly ILogger<BlogManagementService> _logger;

        public BlogManagementService(
            IBlogServiceClient blogServiceClient,
            IUserServiceClient userServiceClient,
            IAuditService auditService,
            ILogger<BlogManagementService> logger)
        {
            _blogServiceClient = blogServiceClient;
            _userServiceClient = userServiceClient;
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
                // Get blog post details before deletion to send notification
                var blogPost = await _blogServiceClient.GetBlogPostByIdAsync(blogPostId);
                
                var success = await _blogServiceClient.DeleteBlogPostAsync(blogPostId, adminId);
                
                if (success)
                {
                    await _auditService.LogActionAsync(adminId, "DELETE_BLOG_POST", "BlogPost", blogPostId,
                        new { Reason = request.Reason }, ipAddress, userAgent);

                    // Send notification to blog author if blog post details were retrieved
                    if (blogPost != null)
                    {
                        try
                        {
                            var notificationTitle = "Your Blog Post Was Removed";
                            var notificationMessage = $"Your blog post \"{blogPost.Title}\" has been removed by an administrator.";
                            
                            await _userServiceClient.CreateNotificationAsync(
                                userId: blogPost.AuthorId,
                                type: "BLOG_REMOVED",
                                title: notificationTitle,
                                message: notificationMessage,
                                reason: request.Reason,
                                source: "ADMIN_ACTION",
                                blogTitle: blogPost.Title,
                                blogId: blogPostId
                            );
                            
                            _logger.LogInformation("Notification sent to user {UserId} for deleted blog post {BlogPostId}", 
                                blogPost.AuthorId, blogPostId);
                        }
                        catch (Exception notificationEx)
                        {
                            _logger.LogError(notificationEx, "Failed to send notification for deleted blog post {BlogPostId}", blogPostId);
                            // Don't fail the whole operation if notification fails
                        }
                    }

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
                // Get blog post details before status update to send notification if needed
                var blogPost = await _blogServiceClient.GetBlogPostByIdAsync(blogPostId);
                
                var success = await _blogServiceClient.UpdateBlogPostStatusAsync(blogPostId, request.Status, adminId);
                
                if (success)
                {
                    await _auditService.LogActionAsync(adminId, "UPDATE_BLOG_POST_STATUS", "BlogPost", blogPostId,
                        new { StatusChanged = request.Status, Reason = request.Reason }, ipAddress, userAgent);

                    // Send notification to blog author for any status change
                    if (blogPost != null && blogPost.Status != request.Status)
                    {
                        try
                        {
                            var previousStatusName = GetStatusName(blogPost.Status);
                            var newStatusName = GetStatusName(request.Status);
                            
                            var (notificationTitle, notificationMessage, notificationType) = GetStatusChangeNotificationDetails(
                                blogPost.Status, request.Status, blogPost.Title, previousStatusName, newStatusName);
                            
                            await _userServiceClient.CreateNotificationAsync(
                                userId: blogPost.AuthorId,
                                type: notificationType,
                                title: notificationTitle,
                                message: notificationMessage,
                                reason: request.Reason,
                                source: "ADMIN_ACTION",
                                blogTitle: blogPost.Title,
                                blogId: blogPostId,
                                additionalData: $"{{\"previousStatus\": \"{previousStatusName}\", \"newStatus\": \"{newStatusName}\", \"timestamp\": \"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\"}}"
                            );
                            
                            _logger.LogInformation("Notification sent to user {UserId} for blog post {BlogPostId} status change from {PreviousStatus} to {NewStatus}", 
                                blogPost.AuthorId, blogPostId, previousStatusName, newStatusName);
                        }
                        catch (Exception notificationEx)
                        {
                            _logger.LogError(notificationEx, "Failed to send notification for blog post {BlogPostId} status change", blogPostId);
                            // Don't fail the whole operation if notification fails
                        }
                    }

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

        private static (string title, string message, string type) GetStatusChangeNotificationDetails(
            int previousStatus, int newStatus, string blogTitle, string previousStatusName, string newStatusName)
        {
            return (previousStatus, newStatus) switch
            {
                // From Draft to Published
                (0, 1) => ("Your Blog Post Was Published",
                          $"Your blog post \"{blogTitle}\" has been published by an administrator.",
                          "BLOG_STATUS_CHANGED"),
                // From Draft to Deleted
                (0, 2) => ("Your Blog Post Was Deleted",
                          $"Your blog post \"{blogTitle}\" has been deleted by an administrator.",
                          "BLOG_REMOVED"),
                // From Published to Draft
                (1, 0) => ("Your Blog Post Was Changed to Draft",
                          $"Your blog post \"{blogTitle}\" has been changed from Published to Draft by an administrator.",
                          "BLOG_STATUS_CHANGED"),
                // From Published to Deleted
                (1, 2) => ("Your Blog Post Was Deleted",
                          $"Your blog post \"{blogTitle}\" has been deleted by an administrator.",
                          "BLOG_REMOVED"),
                // From Deleted to Draft
                (2, 0) => ("Your Blog Post Was Restored to Draft",
                          $"Your blog post \"{blogTitle}\" has been restored from deleted status to draft by an administrator.",
                          "BLOG_STATUS_CHANGED"),
                // From Deleted to Published
                (2, 1) => ("Your Blog Post Was Restored and Published",
                          $"Your blog post \"{blogTitle}\" has been restored from deleted status and published by an administrator.",
                          "BLOG_STATUS_CHANGED"),
                // Any other status change
                _ => ("Your Blog Post Status Was Changed",
                      $"Your blog post \"{blogTitle}\" status has been changed from {previousStatusName} to {newStatusName} by an administrator.",
                      "BLOG_STATUS_CHANGED")
            };
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
