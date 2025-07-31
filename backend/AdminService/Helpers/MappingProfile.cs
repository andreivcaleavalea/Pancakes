using AutoMapper;
using AdminService.Models.Entities;
using AdminService.Models.DTOs;
using System.Text.Json;

namespace AdminService.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // AdminUser mappings
            CreateMap<AdminUser, AdminUserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));

            CreateMap<AdminRole, AdminRoleDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom<PermissionConverter>());

            // Audit log mappings
            CreateMap<AdminAuditLog, AdminAuditLogDto>();

            // Content moderation mappings
            CreateMap<ContentFlag, ContentFlagDto>();
            CreateMap<UserReport, UserReportDto>();

            // System metrics mappings
            CreateMap<SystemMetric, SystemMetricDto>();

            // Analytics mappings
            CreateMap<SystemMetric, DashboardStatsDto>()
                .ForMember(dest => dest.UserStats, opt => opt.MapFrom(src => new UserStatsDto
                {
                    TotalUsers = src.TotalUsers,
                    ActiveUsers = src.ActiveUsers,
                    OnlineUsers = src.OnlineUsers,
                    DailySignups = src.DailySignups
                }))
                .ForMember(dest => dest.ContentStats, opt => opt.MapFrom(src => new ContentStatsDto
                {
                    TotalBlogPosts = src.TotalBlogPosts,
                    BlogPostsToday = src.BlogPostsCreatedToday,
                    TotalComments = src.TotalComments,
                    CommentsToday = src.CommentsPostedToday
                }))
                .ForMember(dest => dest.ModerationStats, opt => opt.MapFrom(src => new ModerationStatsDto
                {
                    TotalReports = src.TotalReports,
                    PendingReports = src.PendingReports,
                    TotalFlags = src.TotalFlags,
                    PendingFlags = src.PendingFlags
                }))
                .ForMember(dest => dest.SystemStats, opt => opt.MapFrom(src => new SystemStatsDto
                {
                    CpuUsage = src.CpuUsage,
                    MemoryUsage = src.MemoryUsage,
                    DiskUsage = src.DiskUsage
                }));
        }
    }

    public class PermissionConverter : IValueResolver<AdminRole, AdminRoleDto, List<string>>
    {
        public List<string> Resolve(AdminRole source, AdminRoleDto destination, List<string> destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.Permissions))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(source.Permissions) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}