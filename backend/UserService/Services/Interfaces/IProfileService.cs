using UserService.Models.DTOs;

namespace UserService.Services.Interfaces;

public interface IProfileService
{
    Task<ProfileDataDto?> GetProfileDataAsync(string userId);
    Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
} 