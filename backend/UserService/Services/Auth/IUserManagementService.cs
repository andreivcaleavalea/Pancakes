using UserService.Models;
using UserService.Models.Entities;
using UserService.Models.Authentication;

namespace UserService.Services.Auth;

public interface IUserManagementService
{
    Task<User> CreateOrUpdateUserFromOAuthAsync(OAuthUserInfo oauthInfo, string provider);
    Task<User?> GetUserByIdAsync(string userId);
    bool ValidateUserId(string userId, string provider, string providerUserId);
} 