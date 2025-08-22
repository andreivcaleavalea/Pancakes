using UserService.Models.Authentication;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UserService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace UserService.Services.Implementations
{
    public class OAuthService : IOAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OAuthService> _logger;

        public OAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<OAuthService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OAuthUserInfo?> ExchangeCodeForUserInfo(string code, string provider)
        {
            try
            {
                var accessToken = await ExchangeCodeForToken(code, provider);
                if (string.IsNullOrEmpty(accessToken))
                    return null;

                return await GetUserInfo(accessToken, provider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OAuth error for provider {Provider}", provider);
                return null;
            }
        }

        public async Task<string?> ExchangeCodeForToken(string code, string provider)
        {
            string tokenUrl;
            if (provider.ToLower() == "google")
                tokenUrl = "https://oauth2.googleapis.com/token";
            else if (provider.ToLower() == "github")
                tokenUrl = "https://github.com/login/oauth/access_token";
            else 
                throw new ArgumentException($"Unsupported provider: {provider}");
            var parameters = GetTokenRequestParameters(code, provider);
            var content = new FormUrlEncodedContent(parameters);

            var response = await _httpClient.PostAsync(tokenUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token exchange failed for provider {Provider}: {ResponseContent}", provider, responseContent);
                return null;
            }

            return ExtractAccessToken(responseContent, provider);
        }

        private Dictionary<string, string> GetTokenRequestParameters(string code, string provider)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("USER_API_BASE_URL") ?? "http://localhost:5141";
            var redirectUri = $"{apiBaseUrl}/auth/{provider.ToLower()}/callback";

            if (provider.ToLower() == "google")
            {
                var clientId = Environment.GetEnvironmentVariable("OAUTH_GOOGLE_CLIENT_ID") 
                    ?? throw new InvalidOperationException("OAUTH_GOOGLE_CLIENT_ID must be set");
                var clientSecret = Environment.GetEnvironmentVariable("OAUTH_GOOGLE_CLIENT_SECRET") 
                    ?? throw new InvalidOperationException("OAUTH_GOOGLE_CLIENT_SECRET must be set");

                return new Dictionary<string, string>
                {
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                    {"code", code},
                    {"grant_type", "authorization_code"},
                    {"redirect_uri", redirectUri}
                };
            }
            else if (provider.ToLower() == "github")
            {
                var clientId = Environment.GetEnvironmentVariable("OAUTH_GITHUB_CLIENT_ID") 
                    ?? throw new InvalidOperationException("OAUTH_GITHUB_CLIENT_ID must be set");
                var clientSecret = Environment.GetEnvironmentVariable("OAUTH_GITHUB_CLIENT_SECRET") 
                    ?? throw new InvalidOperationException("OAUTH_GITHUB_CLIENT_SECRET must be set");

                return new Dictionary<string, string>
                {
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                    {"code", code},
                    {"redirect_uri", redirectUri}
                };
            }

            throw new ArgumentException($"Unsupported provider: {provider}");
        }

        private string? ExtractAccessToken(string response, string provider)
        {
            try
            {
                if (provider.ToLower() == "google")
                {
                    // Google returns JSON
                    var json = JsonDocument.Parse(response);
                    return json.RootElement.GetProperty("access_token").GetString();
                }
                if (provider.ToLower() == "github")
                {
                    // GitHub returns URL-encoded form
                    var parameters = System.Web.HttpUtility.ParseQueryString(response);
                    return parameters["access_token"];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract access token for provider {Provider}", provider);
            }
            
            return null;
        }

        public async Task<OAuthUserInfo?> GetUserInfo(string accessToken, string provider)
        {
            // Clear any existing headers to prevent conflicts
            _httpClient.DefaultRequestHeaders.Clear();

            string userInfoUrl;
            if (provider.ToLower() == "google")
            {
                userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
            }
            else if (provider.ToLower() == "github")
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "PancakesBlog/1.0");
                userInfoUrl = "https://api.github.com/user";
            }
            else
            {
                _logger.LogWarning("Unsupported OAuth provider: {Provider}", provider);
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(userInfoUrl);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info for provider {Provider}: {Content}", provider, content);
                return null;
            }

            var userInfo = ParseUserInfo(content, provider);
            
            // For GitHub, get email separately if not provided in user info
            if (provider.ToLower() == "github" && userInfo != null && string.IsNullOrEmpty(userInfo.Email))
            {
                userInfo.Email = await GetGitHubUserEmail(accessToken);
            }

            return userInfo;
        }

        private OAuthUserInfo? ParseUserInfo(string jsonContent, string provider)
        {
            try
            {
                var json = JsonDocument.Parse(jsonContent);
                var root = json.RootElement;

                // Handle provider-specific field extraction
                string id = GetIdField(root, provider);
                string name = GetNameField(root, provider);
                string email = GetEmailField(root, provider);
                string picture = GetPictureField(root, provider);

                return new OAuthUserInfo
                {
                    Id = id,
                    Name = name,
                    Email = email,
                    Picture = picture
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse user info for provider {Provider}", provider);
                _logger.LogDebug("JSON content for provider {Provider}: {JsonContent}", provider, jsonContent);
            }
            
            return null;
        }

        private string GetIdField(JsonElement root, string provider)
        {
            if (provider.ToLower() == "github")
            {
                // GitHub returns ID as number
                return root.GetProperty("id").GetInt64().ToString();
            }
            // Google returns ID as string
            return root.GetProperty("id").GetString() ?? "";
        }

        private string GetNameField(JsonElement root, string provider)
        {
            if (provider.ToLower() == "github")
            {
                // GitHub: use 'name' if available, fallback to 'login'
                if (root.TryGetProperty("name", out var nameElement) && nameElement.ValueKind != JsonValueKind.Null)
                {
                    return nameElement.GetString() ?? "";
                }
                return root.GetProperty("login").GetString() ?? "";
            }
            // Google
            return root.GetProperty("name").GetString() ?? "";
        }

        private string GetEmailField(JsonElement root, string provider)
        {
            if (provider.ToLower() == "github")
            {
                // GitHub: email might be null
                if (root.TryGetProperty("email", out var emailElement) && emailElement.ValueKind != JsonValueKind.Null)
                {
                    return emailElement.GetString() ?? "";
                }
                return "";
            }
            // Google
            return root.GetProperty("email").GetString() ?? "";
        }

        private string GetPictureField(JsonElement root, string provider)
        {
            if (provider.ToLower() == "github")
            {
                // GitHub uses 'avatar_url'
                return root.TryGetProperty("avatar_url", out var avatarElement) 
                    ? avatarElement.GetString() ?? "" 
                    : "";
            }
            // Google uses 'picture'
            return root.GetProperty("picture").GetString() ?? "";
        }

        private async Task<string> GetGitHubUserEmail(string accessToken)
        {
            try
            {
                // Create a new HTTP request for emails endpoint
                // Note: The authorization header should already be set from the previous call
                var emailResponse = await _httpClient.GetAsync("https://api.github.com/user/emails");
                
                if (!emailResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get GitHub user emails: {StatusCode}", emailResponse.StatusCode);
                    var errorContent = await emailResponse.Content.ReadAsStringAsync();
                    _logger.LogDebug("GitHub email error details: {ErrorContent}", errorContent);
                    return "";
                }

                var emailContent = await emailResponse.Content.ReadAsStringAsync();
                _logger.LogDebug("GitHub emails response: {EmailContent}", emailContent);
                
                var emailJson = JsonDocument.Parse(emailContent);

                // Find the primary email or the first verified email
                foreach (var emailElement in emailJson.RootElement.EnumerateArray())
                {
                    var email = emailElement.GetProperty("email").GetString() ?? "";
                    var primary = emailElement.TryGetProperty("primary", out var primaryProp) && primaryProp.GetBoolean();
                    var verified = emailElement.TryGetProperty("verified", out var verifiedProp) && verifiedProp.GetBoolean();

                    // Return primary email if found and verified
                    if (primary && verified)
                    {
                        _logger.LogDebug("Found primary verified email: {Email}", email);
                        return email;
                    }
                }

                // If no primary email, return the first verified email
                foreach (var emailElement in emailJson.RootElement.EnumerateArray())
                {
                    var email = emailElement.GetProperty("email").GetString() ?? "";
                    var verified = emailElement.TryGetProperty("verified", out var verifiedProp) && verifiedProp.GetBoolean();

                    if (verified && !string.IsNullOrEmpty(email))
                    {
                        _logger.LogDebug("Found verified email: {Email}", email);
                        return email;
                    }
                }

                _logger.LogWarning("No verified email found for GitHub user");
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GitHub user email");
                return "";
            }
        }
    }
}