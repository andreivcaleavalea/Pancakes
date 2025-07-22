using UserService.Models;
using UserService.Models.DTOs;

namespace UserService.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(string id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByProviderAndProviderUserIdAsync(string provider, string providerUserId);
    Task<IEnumerable<UserDto>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<UserDto> CreateAsync(CreateUserDto createDto);
    Task<UserDto> UpdateAsync(string id, UpdateUserDto updateDto);
    Task DeleteAsync(string id);
    Task<UserDto> CreateOrUpdateFromOAuthAsync(OAuthUserInfo oauthInfo, string provider);
}
