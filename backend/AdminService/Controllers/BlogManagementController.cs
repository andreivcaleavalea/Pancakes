using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AdminService.Validations;
using AdminService.Extensions;
using AdminService.Clients.BlogClient.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BlogManagementController(
        IBlogManagementService blogManagementService,
        ILogger<BlogManagementController> logger)
        : ControllerBase
    {
        [HttpGet("search")]
        [Authorize(Policy = "CanViewBlogs")]
        public async Task<IActionResult> SearchBlogPosts([FromQuery] BlogPostSearchRequest request)
        {
            var result = await blogManagementService.SearchBlogPostsAsync(request);
            
            if (result.Success)
            {
                return Ok(new ApiResponse<PagedResponse<BlogPostDTO>>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            logger.LogError("Error searching blog posts: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpDelete("posts/{blogPostId}")]
        [Authorize(Policy = "CanDeleteBlogs")]
        public async Task<IActionResult> DeleteBlogPost(string blogPostId, [FromBody] DeleteBlogPostRequest request)
        {
            var validationResult = BlogManagementRequestValidator.ValidateDeleteBlogPostRequest(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.ToList()
                });
            }

            var currentAdminId = this.GetCurrentAdminId();
            if (string.IsNullOrEmpty(currentAdminId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Admin ID not found in token"
                });
            }

            var result = await blogManagementService.DeleteBlogPostAsync(
                blogPostId, request, currentAdminId, this.GetClientIpAddress(), this.GetUserAgent());
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Data ?? "Blog post deleted successfully"
                });
            }

            if (result.Message == "Failed to delete blog post")
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            logger.LogError("Error deleting blog post {BlogPostId}: {Errors}", blogPostId, string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpPut("posts/{blogPostId}/status")]
        [Authorize(Policy = "CanManageBlogs")]
        public async Task<IActionResult> UpdateBlogPostStatus(string blogPostId, [FromBody] UpdateBlogPostStatusRequest request)
        {
            request.BlogPostId = blogPostId;
            
            var validationResult = BlogManagementRequestValidator.ValidateUpdateBlogPostStatusRequest(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.ToList()
                });
            }

            var currentAdminId = this.GetCurrentAdminId();
            if (string.IsNullOrEmpty(currentAdminId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Admin ID not found in token"
                });
            }

            var result = await blogManagementService.UpdateBlogPostStatusAsync(
                blogPostId, request, currentAdminId, this.GetClientIpAddress(), this.GetUserAgent());
            
            if (result.Success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Data ?? "Blog post status updated successfully"
                });
            }

            if (result.Message == "Failed to update blog post status")
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            logger.LogError("Error updating blog post status {BlogPostId}: {Errors}", blogPostId, string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }

        [HttpGet("statistics")]
        [Authorize(Policy = "CanViewAnalytics")]
        public async Task<IActionResult> GetBlogStatistics()
        {
            var result = await blogManagementService.GetBlogStatisticsAsync();
            
            if (result.Success)
            {
                return Ok(new ApiResponse<Dictionary<string, object>>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }

            logger.LogError("Error getting blog statistics: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = result.Message
            });
        }
    }
}
