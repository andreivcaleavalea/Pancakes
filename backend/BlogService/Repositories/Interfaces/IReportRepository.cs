using BlogService.Models.Entities;

namespace BlogService.Repositories.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id);
    Task<IEnumerable<Report>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20);
    Task<IEnumerable<Report>> GetByReporterIdAsync(string reporterId);
    Task<IEnumerable<Report>> GetByReportedUserIdAsync(string reportedUserId);
    Task<IEnumerable<Report>> GetByContentIdAsync(Guid contentId, ReportContentType contentType);
    Task<Report> CreateAsync(Report report);
    Task<Report> UpdateAsync(Report report);
    Task DeleteAsync(Guid id);
    Task<int> GetTotalCountAsync();
    Task<int> GetPendingCountAsync();
    Task<bool> HasUserReportedContentAsync(string userId, Guid contentId, ReportContentType contentType);
}
