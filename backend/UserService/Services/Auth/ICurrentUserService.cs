using UserService.Models;

namespace UserService.Services.Auth;

public interface ICurrentUserService
{
    User? GetCurrentUser();
    string? GetCurrentUserId();
    bool IsAuthenticated();
} 