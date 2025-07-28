using UserService.Models.Entities;

namespace UserService.Repositories.Interfaces;

public interface IEducationRepository
{
    Task<Education?> GetByIdAsync(string id);
    Task<IEnumerable<Education>> GetByUserIdAsync(string userId);
    Task<Education> CreateAsync(Education education);
    Task<Education> UpdateAsync(Education education);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
} 