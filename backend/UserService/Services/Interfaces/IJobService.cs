using UserService.Models.DTOs;

namespace UserService.Services.Interfaces;

public interface IJobService
{
    Task<IEnumerable<JobDto>> GetUserJobsAsync(string userId);
    Task<JobDto> CreateAsync(string userId, JobDto jobDto);
    Task<JobDto> UpdateAsync(string userId, string jobId, JobDto jobDto);
    Task DeleteAsync(string userId, string jobId);
} 