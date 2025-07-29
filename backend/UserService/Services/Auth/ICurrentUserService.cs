using UserService.Models;
using UserService.Models.Entities;

namespace UserService.Services.Auth;

public interface ICurrentUserService
{
    User? GetCurrentUser();
    string? GetCurrentUserId();
    bool IsAuthenticated();
} 