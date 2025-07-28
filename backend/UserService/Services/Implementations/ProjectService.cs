using AutoMapper;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId)
    {
        await EnsureUserExistsAsync(userId);
        var projects = await _projectRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<ProjectDto>>(projects);
    }

    public async Task<ProjectDto> CreateAsync(string userId, ProjectDto projectDto)
    {
        await EnsureUserExistsAsync(userId);
        
        var project = _mapper.Map<Project>(projectDto);
        project.UserId = userId;
        
        var createdProject = await _projectRepository.CreateAsync(project);
        return _mapper.Map<ProjectDto>(createdProject);
    }

    public async Task<ProjectDto> UpdateAsync(string userId, string projectId, ProjectDto projectDto)
    {
        await EnsureUserExistsAsync(userId);
        var existingProject = await GetProjectByIdOrThrowAsync(projectId, userId);
        
        _mapper.Map(projectDto, existingProject);
        var updatedProject = await _projectRepository.UpdateAsync(existingProject);
        return _mapper.Map<ProjectDto>(updatedProject);
    }

    public async Task DeleteAsync(string userId, string projectId)
    {
        await EnsureUserExistsAsync(userId);
        await GetProjectByIdOrThrowAsync(projectId, userId);
        await _projectRepository.DeleteAsync(projectId);
    }

    private async Task<Project> GetProjectByIdOrThrowAsync(string projectId, string userId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null || project.UserId != userId)
        {
            throw new ArgumentException($"Project with ID {projectId} not found for user {userId}.");
        }
        return project;
    }

    private async Task EnsureUserExistsAsync(string userId)
    {
        if (!await _userRepository.ExistsAsync(userId))
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }
    }
} 