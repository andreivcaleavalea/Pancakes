using UserService.Models.DTOs;

namespace UserService.Services.Interfaces;

public interface IHobbyService
{
    Task<IEnumerable<HobbyDto>> GetUserHobbiesAsync(string userId);
    Task<HobbyDto> CreateAsync(string userId, HobbyDto hobbyDto);
    Task<HobbyDto> UpdateAsync(string userId, string hobbyId, HobbyDto hobbyDto);
    Task DeleteAsync(string userId, string hobbyId);
} 