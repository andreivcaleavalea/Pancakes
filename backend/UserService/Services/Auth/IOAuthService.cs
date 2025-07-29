using UserService.Models;
using UserService.Models.Entities;
using UserService.Models.Authentication;

namespace UserService.Services.Auth;

public interface IOAuthService
{
    Task<OAuthUserInfo?> ExchangeCodeForUserInfo(string code, string provider);
} 