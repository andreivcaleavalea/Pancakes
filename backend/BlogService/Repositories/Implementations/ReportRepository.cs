using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class ReportRepository : IReportRepository
{
    private readonly BlogDbContext _context;

    public ReportRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Report>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Reports
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20)
    {
        return await _context.Reports
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByReporterIdAsync(string reporterId)
    {
        return await _context.Reports
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByReportedUserIdAsync(string reportedUserId)
    {
        return await _context.Reports
            .Where(r => r.ReportedUserId == reportedUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByContentIdAsync(Guid contentId, ReportContentType contentType)
    {
        return await _context.Reports
            .Where(r => r.ContentId == contentId && r.ContentType == contentType)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Report> CreateAsync(Report report)
    {
        report.CreatedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        
        // Reload the saved report
        return await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == report.Id) ?? report;
    }

    public async Task<Report> UpdateAsync(Report report)
    {
        report.UpdatedAt = DateTime.UtcNow;
        _context.Reports.Update(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var report = await GetByIdAsync(id);
        if (report != null)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Reports.CountAsync();
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _context.Reports
            .CountAsync(r => r.Status == ReportStatus.Pending);
    }

    public async Task<bool> HasUserReportedContentAsync(string userId, Guid contentId, ReportContentType contentType)
    {
        return await _context.Reports
            .AnyAsync(r => r.ReporterId == userId && 
                          r.ContentId == contentId && 
                          r.ContentType == contentType);
    }
}
