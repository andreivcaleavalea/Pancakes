using UserService.Models.Authentication;

namespace UserService.Services.Interfaces;

public interface IOAuthService
{
    Task<OAuthUserInfo?> ExchangeCodeForUserInfo(string code, string provider);
    Task<string?> ExchangeCodeForToken(string code, string provider);
    Task<OAuthUserInfo?> GetUserInfo(string accessToken, string provider);
}
