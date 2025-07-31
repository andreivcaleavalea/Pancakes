using AdminService.Models.DTOs;
using AdminService.Services.Interfaces;
using System.Text.Json;

namespace AdminService.Clients
{
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;
        private readonly IServiceJwtService _serviceJwtService;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger, IServiceJwtService serviceJwtService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceJwtService = serviceJwtService;

            var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:5141";
            _httpClient.BaseAddress = new Uri(userServiceUrl);
        }

        private void AddAuthenticationHeader()
        {
            var token = _serviceJwtService.GenerateServiceToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<UserOverviewDto?> GetUserAsync(string userId)
        {
            try
            {
                AddAuthenticationHeader();
                var response = await _httpClient.GetAsync($"/users/{userId}");
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
                AddAuthenticationHeader();
                var response = await _httpClient.GetAsync($"/users/{userId}");
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
                AddAuthenticationHeader();
                var query = $"?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                    query += $"&search={Uri.EscapeDataString(searchTerm)}";

                var response = await _httpClient.GetAsync($"/users{query}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get users from UserService. Status: {StatusCode}", response.StatusCode);
                    return new List<UserOverviewDto>();
                }

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
                AddAuthenticationHeader();
                var request = new
                {
                    userId,
                    reason,
                    expiresAt,
                    bannedBy = "AdminService"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/users/{userId}/ban", content);
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
                AddAuthenticationHeader();
                var request = new
                {
                    userId,
                    reason,
                    unbannedBy = "AdminService"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/users/{userId}/unban", content);
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
                AddAuthenticationHeader();
                var response = await _httpClient.GetAsync("/users/statistics");
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