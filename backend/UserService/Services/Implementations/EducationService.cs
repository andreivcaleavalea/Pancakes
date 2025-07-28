using AutoMapper;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class EducationService : IEducationService
{
    private readonly IEducationRepository _educationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public EducationService(
        IEducationRepository educationRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _educationRepository = educationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EducationDto>> GetUserEducationsAsync(string userId)
    {
        await EnsureUserExistsAsync(userId);
        var educations = await _educationRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<EducationDto>>(educations);
    }

    public async Task<EducationDto> CreateAsync(string userId, EducationDto educationDto)
    {
        await EnsureUserExistsAsync(userId);
        
        var education = _mapper.Map<Education>(educationDto);
        education.UserId = userId;
        
        var createdEducation = await _educationRepository.CreateAsync(education);
        return _mapper.Map<EducationDto>(createdEducation);
    }

    public async Task<EducationDto> UpdateAsync(string userId, string educationId, EducationDto educationDto)
    {
        await EnsureUserExistsAsync(userId);
        var existingEducation = await GetEducationByIdOrThrowAsync(educationId, userId);
        
        _mapper.Map(educationDto, existingEducation);
        var updatedEducation = await _educationRepository.UpdateAsync(existingEducation);
        return _mapper.Map<EducationDto>(updatedEducation);
    }

    public async Task DeleteAsync(string userId, string educationId)
    {
        await EnsureUserExistsAsync(userId);
        await GetEducationByIdOrThrowAsync(educationId, userId);
        await _educationRepository.DeleteAsync(educationId);
    }

    private async Task<Education> GetEducationByIdOrThrowAsync(string educationId, string userId)
    {
        var education = await _educationRepository.GetByIdAsync(educationId);
        if (education == null || education.UserId != userId)
        {
            throw new ArgumentException($"Education with ID {educationId} not found for user {userId}.");
        }
        return education;
    }

    private async Task EnsureUserExistsAsync(string userId)
    {
        if (!await _userRepository.ExistsAsync(userId))
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }
    }
} 