using TestMicroservice.Models;
using System.Text.Json;

namespace TestMicroservice.Services
{
    public class OAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<OAuthUserInfo?> ExchangeCodeForUserInfo(string code, string provider)
        {
            try
            {
                // Only support Google for now
                if (provider.ToLower() != "google")
                {
                    Console.WriteLine($"Provider {provider} is not yet supported. Only Google authentication is available.");
                    return null;
                }

                var accessToken = await ExchangeCodeForToken(code, provider);
                if (string.IsNullOrEmpty(accessToken))
                    return null;

                return await GetUserInfo(accessToken, provider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OAuth error for {provider}: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> ExchangeCodeForToken(string code, string provider)
        {
            if (provider.ToLower() != "google")
                return null;

            var tokenUrl = "https://oauth2.googleapis.com/token";
            var parameters = GetTokenRequestParameters(code, provider);
            var content = new FormUrlEncodedContent(parameters);

            var response = await _httpClient.PostAsync(tokenUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Token exchange failed for {provider}: {responseContent}");
                return null;
            }

            return ExtractAccessToken(responseContent, provider);
        }

        private Dictionary<string, string> GetTokenRequestParameters(string code, string provider)
        {
            var redirectUri = $"http://localhost:5141/auth/{provider.ToLower()}/callback";

            if (provider.ToLower() == "google")
            {
                return new Dictionary<string, string>
                {
                    {"client_id", _configuration["OAuth:Google:ClientId"]!},
                    {"client_secret", _configuration["OAuth:Google:ClientSecret"]!},
                    {"code", code},
                    {"grant_type", "authorization_code"},
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to extract access token for {provider}: {ex.Message}");
            }
            
            return null;
        }

        private async Task<OAuthUserInfo?> GetUserInfo(string accessToken, string provider)
        {
            if (provider.ToLower() != "google")
                return null;

            var userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(userInfoUrl);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get user info for {provider}: {content}");
                return null;
            }

            return ParseUserInfo(content, provider);
        }

        private OAuthUserInfo? ParseUserInfo(string jsonContent, string provider)
        {
            try
            {
                if (provider.ToLower() == "google")
                {
                    var json = JsonDocument.Parse(jsonContent);
                    var root = json.RootElement;

                    return new OAuthUserInfo
                    {
                        Id = root.GetProperty("id").GetString() ?? "",
                        Name = root.GetProperty("name").GetString() ?? "",
                        Email = root.GetProperty("email").GetString() ?? "",
                        Picture = root.GetProperty("picture").GetString() ?? ""
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse user info for {provider}: {ex.Message}");
            }
            
            return null;
        }

        // Commented out GitHub and Facebook methods for future implementation
        /*
        private async Task<string?> GetGitHubEmail()
        {
            // GitHub email implementation would go here
            return null;
        }
        */
    }
}