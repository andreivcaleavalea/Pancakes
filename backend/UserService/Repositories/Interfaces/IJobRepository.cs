using UserService.Models.Entities;

namespace UserService.Repositories.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(string id);
    Task<IEnumerable<Job>> GetByUserIdAsync(string userId);
    Task<Job> CreateAsync(Job job);
    Task<Job> UpdateAsync(Job job);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
} 