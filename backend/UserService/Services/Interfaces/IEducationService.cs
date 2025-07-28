using UserService.Models.DTOs;

namespace UserService.Services.Interfaces;

public interface IEducationService
{
    Task<IEnumerable<EducationDto>> GetUserEducationsAsync(string userId);
    Task<EducationDto> CreateAsync(string userId, EducationDto educationDto);
    Task<EducationDto> UpdateAsync(string userId, string educationId, EducationDto educationDto);
    Task DeleteAsync(string userId, string educationId);
} 