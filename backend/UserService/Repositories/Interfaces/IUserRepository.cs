using UserService.Models.Entities;

namespace UserService.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByProviderUserIdAsync(string provider, string providerUserId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
} 