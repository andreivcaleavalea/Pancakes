using AdminService.Models.DTOs;
using System.Text.Json;

namespace AdminService.Clients
{
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:5141";
            _httpClient.BaseAddress = new Uri(userServiceUrl);
        }

        public async Task<UserOverviewDto?> GetUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                return JsonSerializer.Deserialize<UserOverviewDto>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId} from UserService", userId);
                return null;
            }
        }

        public async Task<UserDetailDto?> GetUserDetailAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}/details");
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                return JsonSerializer.Deserialize<UserDetailDto>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details {UserId} from UserService", userId);
                return null;
            }
        }

        public async Task<List<UserOverviewDto>> SearchUsersAsync(string? searchTerm, int page, int pageSize)
        {
            try
            {
                var query = $"?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                    query += $"&search={Uri.EscapeDataString(searchTerm)}";

                var response = await _httpClient.GetAsync($"/api/users/search{query}");
                if (!response.IsSuccessStatusCode)
                    return new List<UserOverviewDto>();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Deserialize<List<UserOverviewDto>>(json, options);
                return result ?? new List<UserOverviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users in UserService");
                return new List<UserOverviewDto>();
            }
        }

        public async Task<bool> BanUserAsync(string userId, string reason, DateTime? expiresAt)
        {
            try
            {
                var request = new
                {
                    userId,
                    reason,
                    expiresAt,
                    bannedBy = "AdminService"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/users/{userId}/ban", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user {UserId} in UserService", userId);
                return false;
            }
        }

        public async Task<bool> UnbanUserAsync(string userId, string reason)
        {
            try
            {
                var request = new
                {
                    userId,
                    reason,
                    unbannedBy = "AdminService"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/users/{userId}/unban", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user {UserId} in UserService", userId);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetUserStatisticsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/users/statistics");
                if (!response.IsSuccessStatusCode)
                    return new Dictionary<string, object>();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                return result ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics from UserService");
                return new Dictionary<string, object>();
            }
        }
    }
}