using AdminService.Clients.BlogClient.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace AdminService.Clients.BlogClient.Services
{
    public class BlogServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BlogServiceClient> _logger;
        private readonly IServiceJwtService _serviceJwtService;

        public BlogServiceClient(HttpClient httpClient, ILogger<BlogServiceClient> logger, IConfiguration configuration, IServiceJwtService serviceJwtService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceJwtService = serviceJwtService;
            var baseUrl = Environment.GetEnvironmentVariable("BLOG_SERVICE_URL") ?? "http://localhost:5001";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private void AddAuthenticationHeader()
        {
            var token = _serviceJwtService.GenerateServiceToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<PagedResponse<BlogPostDTO>> SearchBlogPostsAsync(BlogPostSearchRequest request)
        {
            try
            {
                AddAuthenticationHeader();
                
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(request.Search))
                    queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");
                
                if (!string.IsNullOrEmpty(request.AuthorId))
                    queryParams.Add($"authorId={Uri.EscapeDataString(request.AuthorId)}");
                
                if (request.Status.HasValue)
                    queryParams.Add($"status={request.Status.Value}");
                
                if (request.Page > 0)
                    queryParams.Add($"page={request.Page}");
                
                if (request.PageSize > 0)
                    queryParams.Add($"pageSize={request.PageSize}");
                
                if (!string.IsNullOrEmpty(request.SortBy))
                    queryParams.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");
                
                if (!string.IsNullOrEmpty(request.SortOrder))
                    queryParams.Add($"sortOrder={Uri.EscapeDataString(request.SortOrder)}");
                
                if (request.DateFrom.HasValue)
                    queryParams.Add($"dateFrom={request.DateFrom.Value:yyyy-MM-ddTHH:mm:ssZ}");
                
                if (request.DateTo.HasValue)
                    queryParams.Add($"dateTo={request.DateTo.Value:yyyy-MM-ddTHH:mm:ssZ}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"/api/BlogPosts{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("BlogService response content: {Content}", content);
                    
                    // Parse the BlogService format
                    using var document = JsonDocument.Parse(content);
                    var root = document.RootElement;
                    
                    var blogPosts = root.GetProperty("data").Deserialize<List<BlogPostDTO>>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<BlogPostDTO>();
                    
                    var paginationElement = root.GetProperty("pagination");
                    var totalCount = paginationElement.GetProperty("totalItems").GetInt32();
                    var page = paginationElement.GetProperty("currentPage").GetInt32();
                    var pageSize = paginationElement.GetProperty("pageSize").GetInt32();
                    var totalPages = paginationElement.GetProperty("totalPages").GetInt32();
                    var hasNextPage = paginationElement.GetProperty("hasNextPage").GetBoolean();
                    var hasPreviousPage = paginationElement.GetProperty("hasPreviousPage").GetBoolean();
                    
                    var result = new PagedResponse<BlogPostDTO>
                    {
                        Data = blogPosts,
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = hasNextPage,
                        HasPreviousPage = hasPreviousPage
                    };
                    
                    _logger.LogInformation("Parsed {Count} blog posts from BlogService", blogPosts.Count);
                    return result;
                }
                
                _logger.LogError("Failed to search blog posts. Status: {StatusCode}", response.StatusCode);
                return new PagedResponse<BlogPostDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching blog posts");
                return new PagedResponse<BlogPostDTO>();
            }
        }

        public async Task<bool> DeleteBlogPostAsync(string blogPostId, string adminId)
        {
            try
            {
                AddAuthenticationHeader();
                
                var response = await _httpClient.DeleteAsync($"/api/BlogPosts/{blogPostId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog post {BlogPostId}", blogPostId);
                return false;
            }
        }

        public async Task<bool> UpdateBlogPostStatusAsync(string blogPostId, int status, string adminId)
        {
            try
            {
                AddAuthenticationHeader();
                
                var json = JsonSerializer.Serialize(new { 
                    Status = status,
                    AdminId = adminId 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"/api/BlogPosts/{blogPostId}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog post status {BlogPostId}", blogPostId);
                return false;
            }
        }

        public Task<Dictionary<string, object>> GetBlogStatisticsAsync()
        {
            try
            {
                // TODO: Implement this endpoint in BlogService or AnalyticsService
                _logger.LogWarning("Blog statistics endpoint not yet implemented");
                return Task.FromResult(new Dictionary<string, object>
                {
                    ["totalPosts"] = 0,
                    ["publishedPosts"] = 0,
                    ["draftPosts"] = 0,
                    ["totalComments"] = 0,
                    ["message"] = "Statistics endpoint not yet implemented"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog statistics");
                return Task.FromResult(new Dictionary<string, object>());
            }
        }
    }
}
