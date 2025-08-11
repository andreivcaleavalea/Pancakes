using BlogService.Models.DTOs;
using BlogService.Models.Entities;

namespace BlogService.Services.Interfaces;

public interface IReportService
{
    Task<ReportDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportDto>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<ReportDto>> GetByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20);
    Task<IEnumerable<ReportDto>> GetByReporterIdAsync(string reporterId);
    Task<IEnumerable<ReportDto>> GetByReportedUserIdAsync(string reportedUserId);
    Task<ReportDto> CreateReportAsync(CreateReportDto createReportDto, string reporterId, string reporterName);
    Task<ReportDto> UpdateReportAsync(Guid id, UpdateReportDto updateReportDto);
    Task<bool> DeleteReportAsync(Guid id);
    Task<ReportStatsDto> GetReportStatsAsync();
    Task<bool> CanUserReportContentAsync(string userId, Guid contentId, ReportContentType contentType);
    Task<(bool CanReport, string ErrorMessage)> ValidateReportAsync(string userId, Guid contentId, ReportContentType contentType);
}
