using UserService.Models.Entities;
using UserService.Models.DTOs;

namespace UserService.Services.Interfaces;

public interface IUserMappingService
{
    User MapUserDtoToUser(UserDto userDto);
}
