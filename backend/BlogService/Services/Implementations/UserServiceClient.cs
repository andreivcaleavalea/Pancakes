using System.Text.Json;
using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using BlogService.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace BlogService.Services.Implementations;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserServiceClient> _logger;
    private readonly IMemoryCache _cache;

    public UserServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<UserServiceClient> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;

        // Set base address for UserService
        var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? Environment.GetEnvironmentVariable("USER_API_BASE_URL") ?? "http://localhost:5141";
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
        // 🚀 CACHE: Check cache first
        var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.UserById, userId);
        if (_cache.TryGetValue(cacheKey, out UserInfoDto? cachedUser))
        {
            return cachedUser;
        }

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
                var user = JsonSerializer.Deserialize<UserInfoDto>(jsonContent, options);
                
                // 🚀 CACHE: Store in cache with medium duration (user data is moderately stable)
                if (user != null)
                {
                    _cache.Set(cacheKey, user, CacheConfig.Duration.Medium);
                }
                
                return user;
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

    public async Task<IEnumerable<UserInfoDto>> GetUsersByIdsAsync(IEnumerable<string> userIds, string? authToken = null)
    {
        try
        {
            if (userIds == null || !userIds.Any())
                return new List<UserInfoDto>();

            var userIdList = userIds.ToList();
            
            // 🚀 CACHE: Check for cached users first
            var cachedUsers = new List<UserInfoDto>();
            var uncachedUserIds = new List<string>();
            
            foreach (var userId in userIdList)
            {
                var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.UserById, userId);
                if (_cache.TryGetValue(cacheKey, out UserInfoDto? cachedUser) && cachedUser != null)
                {
                    cachedUsers.Add(cachedUser);
                }
                else
                {
                    uncachedUserIds.Add(userId);
                }
            }

            // If all users are cached, return immediately! 🚀
            if (!uncachedUserIds.Any())
            {
                _logger.LogInformation("🚀 All {UserCount} users found in cache - zero API calls needed!", userIdList.Count);
                return cachedUsers;
            }

            _logger.LogInformation("🚀 Cache hit: {CachedCount}/{TotalCount} users, fetching {UncachedCount} from API", 
                cachedUsers.Count, userIdList.Count, uncachedUserIds.Count);

            // Fetch only uncached users from API
            var fetchedUsers = new List<UserInfoDto>();
            
            // Set authorization header if token is provided
            if (!string.IsNullOrEmpty(authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }

            // Create JSON content for the request body
            var jsonContent = JsonSerializer.Serialize(uncachedUserIds);
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/users/batch", httpContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                fetchedUsers = JsonSerializer.Deserialize<List<UserInfoDto>>(responseContent, options) ?? new List<UserInfoDto>();
                
                // 🚀 CACHE: Cache all newly fetched users
                foreach (var user in fetchedUsers)
                {
                    var cacheKey = CacheConfig.FormatKey(CacheConfig.Keys.UserById, user.Id);
                    _cache.Set(cacheKey, user, CacheConfig.Duration.Medium);
                }
            }
            else
            {
                _logger.LogWarning("Failed to get users by IDs from UserService. Status: {StatusCode}", response.StatusCode);
            }

            // Combine cached and fetched users
            var allUsers = cachedUsers.Concat(fetchedUsers).ToList();
            
            _logger.LogInformation("✅ Batch user lookup complete: {CachedCount} cached + {FetchedCount} fetched = {TotalCount} users", 
                cachedUsers.Count, fetchedUsers.Count, allUsers.Count);
                
            return allUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService to get users by IDs");
            return new List<UserInfoDto>();
        }
        finally
        {
            // Clear the authorization header to avoid affecting other requests
            if (!string.IsNullOrEmpty(authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
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

    public async Task<bool> AreFriendsAsync(string userId1, string userId2, string authToken)
    {
        try
        {
            // Set authorization header for this request
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.GetAsync($"/api/friendships/check-friendship/{userId2}");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(jsonContent);
                    if (doc.RootElement.TryGetProperty("areFriends", out var areFriendsElement))
                    {
                        if (areFriendsElement.ValueKind == JsonValueKind.True || areFriendsElement.ValueKind == JsonValueKind.False)
                        {
                            return areFriendsElement.GetBoolean();
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Invalid JSON when parsing AreFriends response: {Json}", jsonContent);
                }
                return false;
            }

            _logger.LogWarning("Failed to check friendship between {UserId1} and {UserId2} from UserService. Status: {StatusCode}", 
                userId1, userId2, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService to check friendship between {UserId1} and {UserId2}", 
                userId1, userId2);
            return false;
        }
        finally
        {
            // Clear the authorization header to avoid affecting other requests
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
