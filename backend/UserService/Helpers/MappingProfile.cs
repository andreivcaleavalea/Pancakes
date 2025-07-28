using AutoMapper;
using System.Text.Json;
using UserService.Models.DTOs;
using UserService.Models.Entities;

namespace UserService.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>()
        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Image))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => 
                src.DateOfBirth.HasValue ? src.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty));
        
        CreateMap<UserProfileDto, User>()
            .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Avatar))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => 
                string.IsNullOrEmpty(src.DateOfBirth) ? (DateTime?)null : DateTime.Parse(src.DateOfBirth)))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Education mappings
        CreateMap<Education, EducationDto>();
        CreateMap<EducationDto, Education>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Job mappings
        CreateMap<Job, JobDto>();
        CreateMap<JobDto, Job>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateUserDto, User>()

        // Hobby mappings
        CreateMap<Hobby, HobbyDto>();
        CreateMap<HobbyDto, Hobby>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderUserId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Friendship mappings
        CreateMap<Friendship, FriendshipDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<FriendshipDto, Friendship>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
                Enum.Parse<FriendshipStatus>(src.Status, true)));
        // Project mappings
        CreateMap<Project, ProjectDto>();
        CreateMap<ProjectDto, Project>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Personal page settings mappings
        CreateMap<PersonalPageSettings, PersonalPageSettingsDto>()
            .ForMember(dest => dest.SectionOrder, opt => opt.MapFrom<SectionOrderResolver>())
            .ForMember(dest => dest.SectionVisibility, opt => opt.MapFrom<SectionVisibilityResolver>())
            .ForMember(dest => dest.SectionTemplates, opt => opt.MapFrom<SectionTemplatesResolver>())
            .ForMember(dest => dest.SectionColors, opt => opt.MapFrom<SectionColorsResolver>())
            .ForMember(dest => dest.SectionAdvancedSettings, opt => opt.MapFrom<SectionAdvancedSettingsResolver>());
    }
}

public class SectionOrderResolver : IValueResolver<PersonalPageSettings, PersonalPageSettingsDto, List<string>>
{
    public List<string> Resolve(PersonalPageSettings source, PersonalPageSettingsDto destination, List<string> destMember, ResolutionContext context)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(source.SectionOrder) ?? new List<string>();
        }
        catch
        {
            return new List<string> { "personal", "education", "jobs", "projects", "hobbies" };
        }
    }
}

public class SectionVisibilityResolver : IValueResolver<PersonalPageSettings, PersonalPageSettingsDto, Dictionary<string, bool>>
{
    public Dictionary<string, bool> Resolve(PersonalPageSettings source, PersonalPageSettingsDto destination, Dictionary<string, bool> destMember, ResolutionContext context)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, bool>>(source.SectionVisibility) ?? new Dictionary<string, bool>();
        }
        catch
        {
            return new Dictionary<string, bool>
            {
                { "personal", true },
                { "education", true },
                { "jobs", true },
                { "projects", true },
                { "hobbies", true }
            };
        }
    }
}

public class SectionTemplatesResolver : IValueResolver<PersonalPageSettings, PersonalPageSettingsDto, Dictionary<string, string>>
{
    public Dictionary<string, string> Resolve(PersonalPageSettings source, PersonalPageSettingsDto destination, Dictionary<string, string> destMember, ResolutionContext context)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(source.SectionTemplates) ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>
            {
                { "personal", "card" },
                { "education", "timeline" },
                { "jobs", "timeline" },
                { "projects", "grid" },
                { "hobbies", "tags" }
            };
        }
    }
}

public class SectionColorsResolver : IValueResolver<PersonalPageSettings, PersonalPageSettingsDto, Dictionary<string, string>>
{
    public Dictionary<string, string> Resolve(PersonalPageSettings source, PersonalPageSettingsDto destination, Dictionary<string, string> destMember, ResolutionContext context)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(source.SectionColors) ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>
            {
                { "personal", "blue" },
                { "education", "green" },
                { "jobs", "blue" },
                { "projects", "purple" },
                { "hobbies", "orange" }
            };
        }
    }
}

public class SectionAdvancedSettingsResolver : IValueResolver<PersonalPageSettings, PersonalPageSettingsDto, Dictionary<string, AdvancedSectionSettings>>
{
    public Dictionary<string, AdvancedSectionSettings> Resolve(PersonalPageSettings source, PersonalPageSettingsDto destination, Dictionary<string, AdvancedSectionSettings> destMember, ResolutionContext context)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, AdvancedSectionSettings>>(source.SectionAdvancedSettings) ?? new Dictionary<string, AdvancedSectionSettings>();
        }
        catch
        {
            return new Dictionary<string, AdvancedSectionSettings>();
        }
    }
} 