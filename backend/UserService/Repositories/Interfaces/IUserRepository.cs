using UserService.Models;

namespace UserService.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByProviderAndProviderUserIdAsync(string provider, string providerUserId);
    Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByProviderAndProviderUserIdAsync(string provider, string providerUserId);
}
