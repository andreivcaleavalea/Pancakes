using UserService.Models;

namespace UserService.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    User? DecodeToken(string token);
    bool ValidateToken(string token);
}
