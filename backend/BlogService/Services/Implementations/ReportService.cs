using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BlogService.Services.Implementations;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reportRepository,
        IBlogPostRepository blogPostRepository,
        ICommentRepository commentRepository,
        IMapper mapper,
        ILogger<ReportService> logger)
    {
        _reportRepository = reportRepository;
        _blogPostRepository = blogPostRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReportDto?> GetByIdAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        return report != null ? await MapToReportDtoAsync(report) : null;
    }

    public async Task<IEnumerable<ReportDto>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var reports = await _reportRepository.GetAllAsync(page, pageSize);
        var reportDtos = new List<ReportDto>();
        
        foreach (var report in reports)
        {
            reportDtos.Add(await MapToReportDtoAsync(report));
        }
        
        return reportDtos;
    }

    public async Task<IEnumerable<ReportDto>> GetByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20)
    {
        var reports = await _reportRepository.GetByStatusAsync(status, page, pageSize);
        var reportDtos = new List<ReportDto>();
        
        foreach (var report in reports)
        {
            reportDtos.Add(await MapToReportDtoAsync(report));
        }
        
        return reportDtos;
    }

    public async Task<IEnumerable<ReportDto>> GetByReporterIdAsync(string reporterId)
    {
        var reports = await _reportRepository.GetByReporterIdAsync(reporterId);
        var reportDtos = new List<ReportDto>();
        
        foreach (var report in reports)
        {
            reportDtos.Add(await MapToReportDtoAsync(report));
        }
        
        return reportDtos;
    }

    public async Task<IEnumerable<ReportDto>> GetByReportedUserIdAsync(string reportedUserId)
    {
        var reports = await _reportRepository.GetByReportedUserIdAsync(reportedUserId);
        var reportDtos = new List<ReportDto>();
        
        foreach (var report in reports)
        {
            reportDtos.Add(await MapToReportDtoAsync(report));
        }
        
        return reportDtos;
    }

    public async Task<ReportDto> CreateReportAsync(CreateReportDto createReportDto, string reporterId, string reporterName)
    {
        // Check if user has already reported this content
        var hasReported = await _reportRepository.HasUserReportedContentAsync(
            reporterId, createReportDto.ContentId, createReportDto.ContentType);
            
        if (hasReported)
        {
            throw new InvalidOperationException("You have already reported this content.");
        }

        // Get the content details and reported user info
        string reportedUserId;
        string? reportedUserName = null;
        
        if (createReportDto.ContentType == ReportContentType.BlogPost)
        {
            var blogPost = await _blogPostRepository.GetByIdAsync(createReportDto.ContentId);
            if (blogPost == null)
            {
                throw new ArgumentException("Blog post not found.");
            }
            
            reportedUserId = blogPost.AuthorId;
            
            // Check if user is trying to report their own content
            if (reportedUserId == reporterId)
            {
                throw new InvalidOperationException("You cannot report your own content.");
            }
        }
        else // Comment
        {
            var comment = await _commentRepository.GetByIdAsync(createReportDto.ContentId);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found.");
            }
            
            reportedUserId = comment.AuthorId;
            reportedUserName = comment.AuthorName;
            
            // Check if user is trying to report their own content
            if (reportedUserId == reporterId)
            {
                throw new InvalidOperationException("You cannot report your own content.");
            }
        }

        var report = new Report
        {
            ReporterId = reporterId,
            ReporterName = reporterName,
            ReportedUserId = reportedUserId,
            ReportedUserName = reportedUserName,
            ContentType = createReportDto.ContentType,
            ContentId = createReportDto.ContentId,
            Reason = createReportDto.Reason,
            Description = createReportDto.Description,
            Status = ReportStatus.Pending
        };

        var createdReport = await _reportRepository.CreateAsync(report);
        
        _logger.LogInformation("Report created: {ReportId} by user {ReporterId} for {ContentType} {ContentId}",
            createdReport.Id, reporterId, createReportDto.ContentType, createReportDto.ContentId);

        return await MapToReportDtoAsync(createdReport);
    }

    public async Task<ReportDto> UpdateReportAsync(Guid id, UpdateReportDto updateReportDto, string reviewedBy)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            throw new ArgumentException("Report not found.");
        }

        report.Status = updateReportDto.Status;
        report.AdminNotes = updateReportDto.AdminNotes;
        report.UserBanned = updateReportDto.UserBanned;
        report.ContentRemoved = updateReportDto.ContentRemoved;
        report.ReviewedBy = reviewedBy;
        report.ReviewedAt = DateTime.UtcNow;

        var updatedReport = await _reportRepository.UpdateAsync(report);
        
        _logger.LogInformation("Report updated: {ReportId} by admin {ReviewedBy} - Status: {Status}",
            id, reviewedBy, updateReportDto.Status);

        return await MapToReportDtoAsync(updatedReport);
    }

    public async Task DeleteReportAsync(Guid id)
    {
        await _reportRepository.DeleteAsync(id);
        _logger.LogInformation("Report deleted: {ReportId}", id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _reportRepository.GetTotalCountAsync();
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _reportRepository.GetPendingCountAsync();
    }

    public async Task<bool> CanUserReportContentAsync(string userId, Guid contentId, ReportContentType contentType)
    {
        var (canReport, _) = await ValidateReportAsync(userId, contentId, contentType);
        return canReport;
    }

    public async Task<(bool CanReport, string ErrorMessage)> ValidateReportAsync(string userId, Guid contentId, ReportContentType contentType)
    {
        // Check if user has already reported this content
        var hasReported = await _reportRepository.HasUserReportedContentAsync(userId, contentId, contentType);
        if (hasReported)
        {
            return (false, "You have already reported this content");
        }

        // Check if content exists and get author info
        if (contentType == ReportContentType.BlogPost)
        {
            var blogPost = await _blogPostRepository.GetByIdAsync(contentId);
            if (blogPost == null)
            {
                return (false, "The blog post you're trying to report does not exist");
            }
            
            if (blogPost.AuthorId == userId)
            {
                return (false, "You cannot report your own blog post");
            }
        }
        else // Comment
        {
            var comment = await _commentRepository.GetByIdAsync(contentId);
            if (comment == null)
            {
                return (false, "The comment you're trying to report does not exist");
            }
            
            if (comment.AuthorId == userId)
            {
                return (false, "You cannot report your own comment");
            }
        }

        return (true, string.Empty);
    }

    private async Task<ReportDto> MapToReportDtoAsync(Report report)
    {
        var dto = _mapper.Map<ReportDto>(report);

        // Manually fetch content details since we don't have navigation properties
        if (report.ContentType == ReportContentType.BlogPost)
        {
            var blogPost = await _blogPostRepository.GetByIdAsync(report.ContentId);
            if (blogPost != null)
            {
                dto.ContentTitle = blogPost.Title;
                dto.ContentExcerpt = blogPost.Content.Length > 150 
                    ? blogPost.Content.Substring(0, 150) + "..." 
                    : blogPost.Content;
            }
        }
        else if (report.ContentType == ReportContentType.Comment)
        {
            var comment = await _commentRepository.GetByIdAsync(report.ContentId);
            if (comment != null)
            {
                dto.ContentTitle = "Comment";
                dto.ContentExcerpt = comment.Content.Length > 150 
                    ? comment.Content.Substring(0, 150) + "..." 
                    : comment.Content;
            }
        }

        return dto;
    }
}
