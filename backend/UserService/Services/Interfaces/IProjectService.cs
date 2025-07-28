using UserService.Models.DTOs;

namespace UserService.Services.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId);
    Task<ProjectDto> CreateAsync(string userId, ProjectDto projectDto);
    Task<ProjectDto> UpdateAsync(string userId, string projectId, ProjectDto projectDto);
    Task DeleteAsync(string userId, string projectId);
} 