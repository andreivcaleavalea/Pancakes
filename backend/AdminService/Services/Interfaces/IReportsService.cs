using AdminService.Models.Responses;

namespace AdminService.Services.Interfaces
{
    public interface IReportsService
    {
        Task<ServiceResult<object>> GetReportsAsync(int page = 1, int pageSize = 20, int? status = null);
        Task<ServiceResult<object>> GetReportByIdAsync(string reportId);
        Task<ServiceResult<string>> UpdateReportAsync(string reportId, object updateData, string adminId, string ipAddress, string userAgent);
        Task<ServiceResult<string>> DeleteReportAsync(string reportId, string adminId, string ipAddress, string userAgent);
        Task<ServiceResult<object>> GetReportStatsAsync();
    }
}
