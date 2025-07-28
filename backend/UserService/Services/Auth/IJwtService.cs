using UserService.Models;

namespace UserService.Services.Auth;

public interface IJwtService
{
    string GenerateToken(User user);
    User? GetUserFromToken(string token);
    bool ValidateToken(string token);
} 