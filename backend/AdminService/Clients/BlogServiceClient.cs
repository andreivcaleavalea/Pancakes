using AdminService.Models.DTOs;
using System.Text.Json;

namespace AdminService.Clients
{
    public class BlogServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BlogServiceClient> _logger;

        public BlogServiceClient(HttpClient httpClient, ILogger<BlogServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var blogServiceUrl = Environment.GetEnvironmentVariable("BLOG_SERVICE_URL") ?? "http://localhost:5001";
            _httpClient.BaseAddress = new Uri(blogServiceUrl);
        }

        public async Task<BlogPostModerationDto?> GetBlogPostAsync(Guid blogPostId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/blogposts/{blogPostId}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                return JsonSerializer.Deserialize<BlogPostModerationDto>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog post {BlogPostId} from BlogService", blogPostId);
                return null;
            }
        }

        public async Task<List<BlogPostModerationDto>> SearchBlogPostsAsync(string? searchTerm, int page, int pageSize)
        {
            try
            {
                var query = $"?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                    query += $"&search={Uri.EscapeDataString(searchTerm)}";

                var response = await _httpClient.GetAsync($"/api/blogposts/search{query}");
                if (!response.IsSuccessStatusCode)
                    return new List<BlogPostModerationDto>();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Deserialize<List<BlogPostModerationDto>>(json, options);
                return result ?? new List<BlogPostModerationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching blog posts in BlogService");
                return new List<BlogPostModerationDto>();
            }
        }

        public async Task<CommentModerationDto?> GetCommentAsync(Guid commentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/comments/{commentId}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                return JsonSerializer.Deserialize<CommentModerationDto>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment {CommentId} from BlogService", commentId);
                return null;
            }
        }

        public async Task<List<CommentModerationDto>> SearchCommentsAsync(string? searchTerm, int page, int pageSize)
        {
            try
            {
                var query = $"?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                    query += $"&search={Uri.EscapeDataString(searchTerm)}";

                var response = await _httpClient.GetAsync($"/api/comments/search{query}");
                if (!response.IsSuccessStatusCode)
                    return new List<CommentModerationDto>();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Deserialize<List<CommentModerationDto>>(json, options);
                return result ?? new List<CommentModerationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching comments in BlogService");
                return new List<CommentModerationDto>();
            }
        }

        public async Task<bool> ModerateBlogPostAsync(Guid blogPostId, string action, string reason)
        {
            try
            {
                var request = new
                {
                    blogPostId,
                    action,
                    reason,
                    moderatedBy = "AdminService"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/blogposts/{blogPostId}/moderate", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating blog post {BlogPostId} in BlogService", blogPostId);
                return false;
            }
        }

        public async Task<bool> ModerateCommentAsync(Guid commentId, string action, string reason)
        {
            try
            {
                var request = new
                {
                    commentId,
                    action,
                    reason,
                    moderatedBy = "AdminService"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/comments/{commentId}/moderate", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating comment {CommentId} in BlogService", commentId);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetContentStatisticsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/statistics");
                if (!response.IsSuccessStatusCode)
                    return new Dictionary<string, object>();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                return result ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content statistics from BlogService");
                return new Dictionary<string, object>();
            }
        }
    }
}