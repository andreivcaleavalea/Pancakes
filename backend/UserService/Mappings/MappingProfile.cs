using AutoMapper;
using UserService.Models.DTOs;
using UserService.Models.Entities;

namespace UserService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
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
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Job mappings
            CreateMap<Job, JobDto>();
            CreateMap<JobDto, Job>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Hobby mappings
            CreateMap<Hobby, HobbyDto>();
            CreateMap<HobbyDto, Hobby>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Project mappings
            CreateMap<Project, ProjectDto>();
            CreateMap<ProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
} 