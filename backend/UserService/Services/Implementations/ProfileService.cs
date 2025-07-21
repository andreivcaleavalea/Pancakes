using AutoMapper;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IEducationRepository _educationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IHobbyRepository _hobbyRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public ProfileService(
        IUserRepository userRepository,
        IEducationRepository educationRepository,
        IJobRepository jobRepository,
        IHobbyRepository hobbyRepository,
        IProjectRepository projectRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _educationRepository = educationRepository;
        _jobRepository = jobRepository;
        _hobbyRepository = hobbyRepository;
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<ProfileDataDto?> GetProfileDataAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var educations = await _educationRepository.GetByUserIdAsync(userId);
        var jobs = await _jobRepository.GetByUserIdAsync(userId);
        var hobbies = await _hobbyRepository.GetByUserIdAsync(userId);
        var projects = await _projectRepository.GetByUserIdAsync(userId);

        return new ProfileDataDto
        {
            User = _mapper.Map<UserProfileDto>(user),
            Educations = _mapper.Map<List<EducationDto>>(educations),
            Jobs = _mapper.Map<List<JobDto>>(jobs),
            Hobbies = _mapper.Map<List<HobbyDto>>(hobbies),
            Projects = _mapper.Map<List<ProjectDto>>(projects)
        };
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto)
    {
        var user = await GetUserByIdOrThrowAsync(userId);
        
        user.Name = updateDto.Name;
        user.Bio = updateDto.Bio;
        user.PhoneNumber = updateDto.PhoneNumber;
        
        if (!string.IsNullOrEmpty(updateDto.DateOfBirth))
        {
            if (DateTime.TryParse(updateDto.DateOfBirth, out DateTime parsedDate))
            {
                user.DateOfBirth = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            }
        }
        else
        {
            user.DateOfBirth = null; 
        }

        var updatedUser = await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserProfileDto>(updatedUser);
    }

    private async Task<User> GetUserByIdOrThrowAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }
        return user;
    }
} 