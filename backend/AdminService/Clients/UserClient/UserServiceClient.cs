using AdminService.Clients.UserClient.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace AdminService.Clients.UserClient
{
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;
        private readonly IServiceJwtService _serviceJwtService;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger, IConfiguration configuration, IServiceJwtService serviceJwtService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceJwtService = serviceJwtService;
            var baseUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:5001";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private void AddAuthenticationHeader()
        {
            var token = _serviceJwtService.GenerateServiceToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<PagedResponse<UserOverviewDto>> SearchUsersAsync(UserSearchRequest request)
        {
            try
            {
                AddAuthenticationHeader();
                
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(request.SearchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
                
                if (request.Page > 0)
                    queryParams.Add($"page={request.Page}");
                
                if (request.PageSize > 0)
                    queryParams.Add($"pageSize={request.PageSize}");
                
                if (request.IsActive.HasValue)
                    queryParams.Add($"isActive={request.IsActive}");
                
                if (request.IsBanned.HasValue)
                    queryParams.Add($"isBanned={request.IsBanned}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"/users{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("UserService response content: {Content}", content);
                    
                    // Parse the UserService format
                    using var document = JsonDocument.Parse(content);
                    var root = document.RootElement;
                    
                    var users = root.GetProperty("users").Deserialize<List<UserOverviewDto>>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<UserOverviewDto>();
                    
                    var totalCount = root.GetProperty("totalCount").GetInt32();
                    var page = root.GetProperty("page").GetInt32();
                    var pageSize = root.GetProperty("pageSize").GetInt32();
                    var totalPages = root.GetProperty("totalPages").GetInt32();
                    
                    var result = new PagedResponse<UserOverviewDto>
                    {
                        Data = users,
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    };
                    
                    _logger.LogInformation("Parsed {Count} users from UserService", users.Count);
                    return result;
                }
                
                _logger.LogError("Failed to search users. Status: {StatusCode}", response.StatusCode);
                return new PagedResponse<UserOverviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return new PagedResponse<UserOverviewDto>();
            }
        }

        public async Task<UserDetailDto?> GetUserDetailAsync(string userId)
        {
            try
            {
                AddAuthenticationHeader();
                
                var response = await _httpClient.GetAsync($"/users/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserDetailDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                _logger.LogError("Failed to get user detail. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> BanUserAsync(BanUserRequest request, string adminId)
        {
            try
            {
                AddAuthenticationHeader();
                
                var json = JsonSerializer.Serialize(new { request.UserId, request.Reason, request.ExpiresAt, AdminId = adminId });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"/users/{request.UserId}/ban", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user {UserId}", request.UserId);
                return false;
            }
        }

        public async Task<bool> UnbanUserAsync(UnbanUserRequest request, string adminId)
        {
            try
            {
                AddAuthenticationHeader();
                
                var json = JsonSerializer.Serialize(new { request.UserId, AdminId = adminId });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"/users/{request.UserId}/unban", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user {UserId}", request.UserId);
                return false;
            }
        }

        public async Task<UserDetailDto?> UpdateUserAsync(string userId, UpdateUserRequest request, string adminId)
        {
            try
            {
                AddAuthenticationHeader();
                
                var json = JsonSerializer.Serialize(new { 
                    request.Email, 
                    request.Name, 
                    request.Bio,
                    request.PhoneNumber,
                    request.DateOfBirth, 
                    request.IsActive,
                    AdminId = adminId 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"/users/{userId}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserDetailDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogError("Failed to update user. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return null;
            }
        }

        public Task<bool> ForcePasswordResetAsync(ForcePasswordResetRequest request, string adminId)
        {
            try
            {
                // TODO: Implement this endpoint in UserService
                _logger.LogWarning("Force password reset endpoint not yet implemented in UserService");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forcing password reset for user {UserId}", request.UserId);
                return Task.FromResult(false);
            }
        }

        public Task<Dictionary<string, object>> GetUserStatisticsAsync()
        {
            try
            {
                // TODO: Implement this endpoint in UserService or AnalyticsService
                _logger.LogWarning("User statistics endpoint not yet implemented");
                return Task.FromResult(new Dictionary<string, object>
                {
                    ["totalUsers"] = 0,
                    ["activeUsers"] = 0,
                    ["bannedUsers"] = 0,
                    ["message"] = "Statistics endpoint not yet implemented"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return Task.FromResult(new Dictionary<string, object>());
            }
        }

        public async Task<bool> CreateNotificationAsync(string userId, string type, string title, string message, string reason, string source, string? blogTitle = null, string? blogId = null, string? additionalData = null)
        {
            try
            {
                AddAuthenticationHeader();
                
                var notificationData = new
                {
                    UserId = userId,
                    Type = type,
                    Title = title,
                    Message = message,
                    BlogTitle = blogTitle,
                    BlogId = blogId,
                    Reason = reason,
                    Source = source,
                    AdditionalData = additionalData
                };
                
                var json = JsonSerializer.Serialize(notificationData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("/api/notifications", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Notification created successfully for user {UserId}", userId);
                    return true;
                }
                
                _logger.LogError("Failed to create notification for user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
                return false;
            }
        }
    }
}
