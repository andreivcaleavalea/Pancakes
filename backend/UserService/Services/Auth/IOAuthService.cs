using UserService.Models;

namespace UserService.Services.Auth;

public interface IOAuthService
{
    Task<OAuthUserInfo?> ExchangeCodeForUserInfo(string code, string provider);
} 