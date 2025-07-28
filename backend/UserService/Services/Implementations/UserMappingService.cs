using UserService.Models;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class UserMappingService : IUserMappingService
{
    public User MapUserDtoToUser(UserDto userDto)
    {
        return new User
        {
            Id = userDto.Id,
            Name = userDto.Name,
            Email = userDto.Email,
            Image = userDto.Image,
            Provider = userDto.Provider,
            ProviderUserId = userDto.ProviderUserId,
            Bio = userDto.Bio,
            PhoneNumber = userDto.PhoneNumber,
            DateOfBirth = userDto.DateOfBirth,
            CreatedAt = userDto.CreatedAt,
            LastLoginAt = userDto.LastLoginAt,
            UpdatedAt = userDto.UpdatedAt
        };
    }
}
