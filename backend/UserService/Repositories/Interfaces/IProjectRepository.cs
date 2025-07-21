using UserService.Models.Entities;

namespace UserService.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(string id);
    Task<IEnumerable<Project>> GetByUserIdAsync(string userId);
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
} 