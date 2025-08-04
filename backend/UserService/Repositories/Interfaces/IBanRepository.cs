using UserService.Models.Entities;

namespace UserService.Repositories.Interfaces
{
    public interface IBanRepository
    {
        Task<Ban?> GetByIdAsync(string id);
        Task<Ban?> GetActiveBanAsync(string userId);
        Task<IEnumerable<Ban>> GetBanHistoryAsync(string userId);
        Task<IEnumerable<Ban>> GetAllActiveBansAsync();
        Task<Ban> CreateAsync(Ban ban);
        Task<Ban> UpdateAsync(Ban ban);
        Task DeleteAsync(string id);
        Task<bool> HasActiveBanAsync(string userId);
        Task<int> GetActiveBansCountAsync();
        Task<IEnumerable<Ban>> GetExpiringBansAsync(DateTime cutoffTime);
    }
}