using AdminService.Models.DTOs;
using AdminService.Services.Interfaces;
using System.Text.Json;

// DTO for UserService response format
public class UserServiceUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Ban status helper properties (calculated from Bans collection)
    public bool IsBanned { get; set; } = false;
    public string? CurrentBanReason { get; set; }
    public DateTime? CurrentBanExpiresAt { get; set; }
}

// UserService response format with pagination
public class UserServiceResponse
{
    public List<UserServiceUserDto> Users { get; set; } = new List<UserServiceUserDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
}

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
                
                var userDto = JsonSerializer.Deserialize<UserServiceUserDto>(json, options);
                return userDto != null ? MapToUserOverviewDto(userDto) : null;
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
                
                var userDto = JsonSerializer.Deserialize<UserServiceUserDto>(json, options);
                return userDto != null ? MapToUserDetailDto(userDto) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details {UserId} from UserService", userId);
                return null;
            }
        }
        
        private UserDetailDto MapToUserDetailDto(UserServiceUserDto userDto)
        {
            return new UserDetailDto
            {
                Id = userDto.Id,
                Name = userDto.Name,
                Email = userDto.Email,
                Provider = userDto.Provider,
                CreatedAt = userDto.CreatedAt,
                LastLoginAt = userDto.LastLoginAt,
                TotalBlogPosts = 0, // TODO: Get from BlogService when available
                TotalComments = 0, // TODO: Get from CommentService when available
                ReportsCount = 0, // TODO: Get from ModerationService when available
                IsBanned = userDto.IsBanned,
                CurrentBanReason = userDto.CurrentBanReason,
                CurrentBanExpiresAt = userDto.CurrentBanExpiresAt,
                CurrentBannedAt = null,
                CurrentBannedBy = null,
                TotalBansCount = 0,
                Bio = userDto.Bio,
                PhoneNumber = userDto.PhoneNumber,
                DateOfBirth = userDto.DateOfBirth,
                Image = userDto.Image,
                Education = new List<UserEducationDto>(), // TODO: Get from UserService when available
                Jobs = new List<UserJobDto>(), // TODO: Get from UserService when available
                Projects = new List<UserProjectDto>(), // TODO: Get from UserService when available
                Hobbies = new List<UserHobbyDto>() // TODO: Get from UserService when available
            };
        }

        public async Task<(List<UserOverviewDto> users, int totalCount)> SearchUsersAsync(string? searchTerm, int page, int pageSize)
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
                    return (new List<UserOverviewDto>(), 0);
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                
                // Deserialize the new response format
                var userResponse = JsonSerializer.Deserialize<UserServiceResponse>(json, options);
                if (userResponse?.Users == null) return (new List<UserOverviewDto>(), 0);
                
                // Map to AdminService's UserOverviewDto format
                var result = userResponse.Users.Select(MapToUserOverviewDto).ToList();
                return (result, userResponse.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users in UserService");
                return (new List<UserOverviewDto>(), 0);
            }
        }
        
        private UserOverviewDto MapToUserOverviewDto(UserServiceUserDto userDto)
        {
            return new UserOverviewDto
            {
                Id = userDto.Id,
                Name = userDto.Name,
                Email = userDto.Email,
                Provider = userDto.Provider,
                CreatedAt = userDto.CreatedAt,
                LastLoginAt = userDto.LastLoginAt,
                TotalBlogPosts = 0, // TODO: Get from BlogService when available
                TotalComments = 0, // TODO: Get from CommentService when available
                ReportsCount = 0, // TODO: Get from ModerationService when available
                IsBanned = userDto.IsBanned,
                CurrentBanReason = userDto.CurrentBanReason,
                CurrentBanExpiresAt = userDto.CurrentBanExpiresAt,
                CurrentBannedAt = null, // Will be populated from ban history if needed
                CurrentBannedBy = null, // Will be populated from ban history if needed
                TotalBansCount = 0 // Will be populated from ban service if needed
            };
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