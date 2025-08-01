using System.Text.Json;
using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Set base address for UserService
        var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:5141";
        _httpClient.BaseAddress = new Uri(userServiceUrl);
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync(string authToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.GetAsync("/auth/me");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return JsonSerializer.Deserialize<UserInfoDto>(jsonContent, options);
            }

            _logger.LogWarning("Failed to get current user from UserService. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService to get current user");
            return null;
        }
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return JsonSerializer.Deserialize<UserInfoDto>(jsonContent, options);
            }

            _logger.LogWarning("Failed to get user {UserId} from UserService. Status: {StatusCode}", userId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService to get user {UserId}", userId);
            return null;
        }
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(string userId, string authToken)
    {
        try
        {
            // Set authorization header for this request
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.GetAsync($"/users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return JsonSerializer.Deserialize<UserInfoDto>(jsonContent, options);
            }

            _logger.LogWarning("Failed to get user {UserId} from UserService. Status: {StatusCode}", userId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService to get user {UserId}", userId);
            return null;
        }
        finally
        {
            // Clear the authorization header to avoid affecting other requests
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<IEnumerable<UserInfoDto>> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/users");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var users = JsonSerializer.Deserialize<List<UserInfoDto>>(jsonContent, options);
                return users ?? new List<UserInfoDto>();
            }

            _logger.LogWarning("Failed to get all users from UserService. Status: {StatusCode}", response.StatusCode);
            return new List<UserInfoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService to get all users");
            return new List<UserInfoDto>();
        }
    }

    public async Task<IEnumerable<FriendDto>> GetUserFriendsAsync(string authToken)
    {
        try
        {
            // Set authorization header for this request
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.GetAsync("/api/friendships/friends");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var friends = JsonSerializer.Deserialize<List<FriendDto>>(jsonContent, options);
                return friends ?? new List<FriendDto>();
            }

            _logger.LogWarning("Failed to get friends from UserService. Status: {StatusCode}", response.StatusCode);
            return new List<FriendDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService to get friends");
            return new List<FriendDto>();
        }
        finally
        {
            // Clear the authorization header to avoid affecting other requests
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
