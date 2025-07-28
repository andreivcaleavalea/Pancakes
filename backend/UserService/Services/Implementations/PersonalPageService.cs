using AutoMapper;
using System.Text.Json;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class PersonalPageService : IPersonalPageService
{
    private readonly IPersonalPageSettingsRepository _settingsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEducationRepository _educationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IHobbyRepository _hobbyRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public PersonalPageService(
        IPersonalPageSettingsRepository settingsRepository,
        IUserRepository userRepository,
        IEducationRepository educationRepository,
        IJobRepository jobRepository,
        IHobbyRepository hobbyRepository,
        IProjectRepository projectRepository,
        IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _userRepository = userRepository;
        _educationRepository = educationRepository;
        _jobRepository = jobRepository;
        _hobbyRepository = hobbyRepository;
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<PersonalPageSettingsDto> GetSettingsAsync(string userId)
    {
        var settings = await _settingsRepository.GetByUserIdAsync(userId);
        
        if (settings == null)
        {
            // Create default settings for new users
            settings = new PersonalPageSettings
            {
                UserId = userId,
                PageSlug = await GenerateUniqueSlugAsync(await GetUserNameAsync(userId), userId)
            };
            
            settings = await _settingsRepository.CreateAsync(settings);
        }

        return _mapper.Map<PersonalPageSettingsDto>(settings);
    }

    public async Task<PersonalPageSettingsDto> UpdateSettingsAsync(string userId, UpdatePersonalPageSettingsDto updateDto)
    {
        var settings = await _settingsRepository.GetByUserIdAsync(userId);
        
        if (settings == null)
        {
            throw new ArgumentException($"Personal page settings not found for user {userId}");
        }

        // Validate page slug uniqueness if provided
        if (!string.IsNullOrEmpty(updateDto.PageSlug) && updateDto.PageSlug != settings.PageSlug)
        {
            if (await _settingsRepository.PageSlugExistsAsync(updateDto.PageSlug, userId))
            {
                throw new ArgumentException("Page slug is already taken. Please choose a different one.");
            }
        }

        // Update fields
        if (updateDto.IsPublic.HasValue)
            settings.IsPublic = updateDto.IsPublic.Value;
            
        if (!string.IsNullOrEmpty(updateDto.PageSlug))
            settings.PageSlug = updateDto.PageSlug;
            
        if (updateDto.SectionOrder != null)
            settings.SectionOrder = JsonSerializer.Serialize(updateDto.SectionOrder);
            
        if (updateDto.SectionVisibility != null)
            settings.SectionVisibility = JsonSerializer.Serialize(updateDto.SectionVisibility);
            
        if (updateDto.SectionTemplates != null)
            settings.SectionTemplates = JsonSerializer.Serialize(updateDto.SectionTemplates);
            
        if (updateDto.SectionColors != null)
            settings.SectionColors = JsonSerializer.Serialize(updateDto.SectionColors);
            
        if (updateDto.SectionAdvancedSettings != null)
            settings.SectionAdvancedSettings = JsonSerializer.Serialize(updateDto.SectionAdvancedSettings);
            
        if (!string.IsNullOrEmpty(updateDto.Theme))
            settings.Theme = updateDto.Theme;
            
        if (!string.IsNullOrEmpty(updateDto.ColorScheme))
            settings.ColorScheme = updateDto.ColorScheme;

        var updatedSettings = await _settingsRepository.UpdateAsync(settings);
        return _mapper.Map<PersonalPageSettingsDto>(updatedSettings);
    }

    public async Task<PublicPersonalPageDto?> GetPublicPageAsync(string pageSlug)
    {
        var settings = await _settingsRepository.GetByPageSlugAsync(pageSlug);
        
        if (settings?.User == null)
            return null;

        var userId = settings.UserId;
        
        var educations = await _educationRepository.GetByUserIdAsync(userId);
        var jobs = await _jobRepository.GetByUserIdAsync(userId);
        var hobbies = await _hobbyRepository.GetByUserIdAsync(userId);
        var projects = await _projectRepository.GetByUserIdAsync(userId);

        return new PublicPersonalPageDto
        {
            User = _mapper.Map<UserProfileDto>(settings.User),
            Educations = _mapper.Map<List<EducationDto>>(educations),
            Jobs = _mapper.Map<List<JobDto>>(jobs),
            Hobbies = _mapper.Map<List<HobbyDto>>(hobbies),
            Projects = _mapper.Map<List<ProjectDto>>(projects),
            Settings = _mapper.Map<PersonalPageSettingsDto>(settings)
        };
    }

    public async Task<string> GenerateUniqueSlugAsync(string baseName, string userId)
    {
        // Create a URL-friendly slug from the base name
        var slug = CreateSlugFromName(baseName);
        var originalSlug = slug;
        var counter = 1;

        // Keep trying until we find a unique slug
        while (await _settingsRepository.PageSlugExistsAsync(slug, userId))
        {
            slug = $"{originalSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private async Task<string> GetUserNameAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.Name ?? "user";
    }

    private static string CreateSlugFromName(string name)
    {
        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("&", "and")
            .Replace("@", "at");
    }
} 