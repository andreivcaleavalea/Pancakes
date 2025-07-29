using UserService.Models.Entities;

namespace UserService.Services.Interfaces;

public interface ICurrentUserService
{
    User? GetCurrentUser();
    string? GetCurrentUserId();
    bool IsAuthenticated();
    string? GetUserEmail();
    string? GetUserName();
}
