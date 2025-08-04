using AdminService.Models.DTOs;

namespace AdminService.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<object> GetDetailedAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}