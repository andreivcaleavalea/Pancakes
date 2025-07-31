using AdminService.Models.DTOs;

namespace AdminService.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<UserStatsDto> GetUserStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ContentStatsDto> GetContentStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ModerationStatsDto> GetModerationStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<SystemStatsDto> GetSystemStatsAsync();
        Task<List<DailyMetricDto>> GetDailyMetricsAsync(string metricType, DateTime fromDate, DateTime toDate);
        Task<List<TopContentDto>> GetTopContentAsync(int count = 10);
        Task<List<ModerationActivityDto>> GetRecentModerationActivityAsync(int count = 20);
        Task<List<ServiceStatusDto>> GetServiceStatusesAsync();
        Task CollectSystemMetricsAsync();
        Task<SystemMetricDto> GetLatestSystemMetricsAsync();
        Task<List<SystemMetricDto>> GetHistoricalMetricsAsync(DateTime fromDate, DateTime toDate);
    }
}