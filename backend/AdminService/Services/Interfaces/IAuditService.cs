using AdminService.Models.DTOs;
using AdminService.Models.Entities;
using AdminService.Models.Responses;

namespace AdminService.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(string adminId, string action, string targetType, string targetId, object? details, string ipAddress, string userAgent);
        Task<PagedResponse<AdminAuditLogDto>> GetAuditLogsAsync(int page, int pageSize, string? adminId = null, string? targetType = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AdminAuditLogDto>> GetRecentActivityAsync(int count = 20);
        Task<Dictionary<string, int>> GetActionStatsAsync(DateTime fromDate, DateTime toDate);
        Task CleanupOldLogsAsync(int retentionDays);
    }
}