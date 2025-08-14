using AdminService.Clients.BlogClient.Services;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using System.Text.Json;

namespace AdminService.Services.Implementations
{
    public class ReportsService : IReportsService
    {
        private readonly IBlogServiceClient _blogServiceClient;
        private readonly IAuditService _auditService;
        private readonly ILogger<ReportsService> _logger;

        public ReportsService(
            IBlogServiceClient blogServiceClient,
            IAuditService auditService,
            ILogger<ReportsService> logger)
        {
            _blogServiceClient = blogServiceClient;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ServiceResult<object>> GetReportsAsync(int page = 1, int pageSize = 20, int? status = null)
        {
            try
            {
                var reportsJson = await _blogServiceClient.GetReportsAsync(page, pageSize, status);
                var reports = JsonSerializer.Deserialize<object>(reportsJson);
                
                return ServiceResult<object>.SuccessResult(reports ?? new object(), "Reports retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports");
                return ServiceResult<object>.FailureResult(
                    "An error occurred while retrieving reports", ex.Message);
            }
        }

        public async Task<ServiceResult<object>> GetReportByIdAsync(string reportId)
        {
            try
            {
                var reportJson = await _blogServiceClient.GetReportByIdAsync(reportId);
                var report = JsonSerializer.Deserialize<object>(reportJson);
                
                return ServiceResult<object>.SuccessResult(report ?? new object(), "Report retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report {ReportId}", reportId);
                return ServiceResult<object>.FailureResult(
                    "An error occurred while retrieving the report", ex.Message);
            }
        }

        public async Task<ServiceResult<string>> UpdateReportAsync(string reportId, object updateData, string adminId, string ipAddress, string userAgent)
        {
            try
            {
                var success = await _blogServiceClient.UpdateReportAsync(reportId, updateData);

                if (!success)
                {
                    return ServiceResult<string>.FailureResult("Failed to update report");
                }

                await _auditService.LogActionAsync(adminId, "REPORT_UPDATED", "Report", reportId,
                    updateData, ipAddress, userAgent);

                return ServiceResult<string>.SuccessResult("Report updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", reportId);
                return ServiceResult<string>.FailureResult(
                    "An error occurred while updating the report", ex.Message);
            }
        }

        public async Task<ServiceResult<string>> DeleteReportAsync(string reportId, string adminId, string ipAddress, string userAgent)
        {
            try
            {
                var success = await _blogServiceClient.DeleteReportAsync(reportId);

                if (!success)
                {
                    return ServiceResult<string>.FailureResult("Failed to delete report");
                }

                await _auditService.LogActionAsync(adminId, "REPORT_DELETED", "Report", reportId,
                    null, ipAddress, userAgent);

                return ServiceResult<string>.SuccessResult("Report deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", reportId);
                return ServiceResult<string>.FailureResult(
                    "An error occurred while deleting the report", ex.Message);
            }
        }

        public async Task<ServiceResult<object>> GetReportStatsAsync()
        {
            try
            {
                var statsJson = await _blogServiceClient.GetReportStatsAsync();
                var stats = JsonSerializer.Deserialize<object>(statsJson);
                
                return ServiceResult<object>.SuccessResult(stats ?? new object(), "Report statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report statistics");
                return ServiceResult<object>.FailureResult(
                    "An error occurred while retrieving report statistics", ex.Message);
            }
        }
    }
}
