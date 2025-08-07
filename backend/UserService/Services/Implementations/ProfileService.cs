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
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly IProfilePictureStrategyFactory _profilePictureStrategyFactory;

    public ProfileService(
        IUserRepository userRepository,
        IEducationRepository educationRepository,
        IJobRepository jobRepository,
        IHobbyRepository hobbyRepository,
        IProjectRepository projectRepository,
        IFileService fileService,
        IMapper mapper,
        IProfilePictureStrategyFactory profilePictureStrategyFactory)
    {
        _userRepository = userRepository;
        _educationRepository = educationRepository;
        _jobRepository = jobRepository;
        _hobbyRepository = hobbyRepository;
        _projectRepository = projectRepository;
        _fileService = fileService;
        _mapper = mapper;
        _profilePictureStrategyFactory = profilePictureStrategyFactory;
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

    public async Task<UserProfileDto> UpdateProfilePictureAsync(string userId, IFormFile profilePicture)
    {
        var user = await GetUserByIdOrThrowAsync(userId);
        
        // Save new profile picture
        var newImagePath = await _fileService.SaveProfilePictureAsync(profilePicture, userId);
        
        // Use strategy pattern to handle user upload (this will handle old image deletion and provider change)
        var strategy = _profilePictureStrategyFactory.GetStrategyForUserUpload();
        strategy.HandleUserUpload(user, newImagePath);
        
        var updatedUser = await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserProfileDto>(updatedUser);
    }
} 