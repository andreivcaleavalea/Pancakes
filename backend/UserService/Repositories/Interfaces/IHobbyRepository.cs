using UserService.Models.Entities;

namespace UserService.Repositories.Interfaces;

public interface IHobbyRepository
{
    Task<Hobby?> GetByIdAsync(string id);
    Task<IEnumerable<Hobby>> GetByUserIdAsync(string userId);
    Task<Hobby> CreateAsync(Hobby hobby);
    Task<Hobby> UpdateAsync(Hobby hobby);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
} 