using UserService.Models;

namespace UserService.Services.Interfaces;

public interface ICurrentUserService
{
    User? GetCurrentUser();
    string? GetCurrentUserId();
    bool IsAuthenticated();
    string? GetUserEmail();
    string? GetUserName();
}
