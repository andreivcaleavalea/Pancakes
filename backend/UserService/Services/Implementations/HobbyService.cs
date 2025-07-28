using AutoMapper;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class HobbyService : IHobbyService
{
    private readonly IHobbyRepository _hobbyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public HobbyService(
        IHobbyRepository hobbyRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _hobbyRepository = hobbyRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<HobbyDto>> GetUserHobbiesAsync(string userId)
    {
        await EnsureUserExistsAsync(userId);
        var hobbies = await _hobbyRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<HobbyDto>>(hobbies);
    }

    public async Task<HobbyDto> CreateAsync(string userId, HobbyDto hobbyDto)
    {
        await EnsureUserExistsAsync(userId);
        
        var hobby = _mapper.Map<Hobby>(hobbyDto);
        hobby.UserId = userId;
        
        var createdHobby = await _hobbyRepository.CreateAsync(hobby);
        return _mapper.Map<HobbyDto>(createdHobby);
    }

    public async Task<HobbyDto> UpdateAsync(string userId, string hobbyId, HobbyDto hobbyDto)
    {
        await EnsureUserExistsAsync(userId);
        var existingHobby = await GetHobbyByIdOrThrowAsync(hobbyId, userId);
        
        _mapper.Map(hobbyDto, existingHobby);
        var updatedHobby = await _hobbyRepository.UpdateAsync(existingHobby);
        return _mapper.Map<HobbyDto>(updatedHobby);
    }

    public async Task DeleteAsync(string userId, string hobbyId)
    {
        await EnsureUserExistsAsync(userId);
        await GetHobbyByIdOrThrowAsync(hobbyId, userId);
        await _hobbyRepository.DeleteAsync(hobbyId);
    }

    private async Task<Hobby> GetHobbyByIdOrThrowAsync(string hobbyId, string userId)
    {
        var hobby = await _hobbyRepository.GetByIdAsync(hobbyId);
        if (hobby == null || hobby.UserId != userId)
        {
            throw new ArgumentException($"Hobby with ID {hobbyId} not found for user {userId}.");
        }
        return hobby;
    }

    private async Task EnsureUserExistsAsync(string userId)
    {
        if (!await _userRepository.ExistsAsync(userId))
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }
    }
} 