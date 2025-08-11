using BlogService.Models.Entities;

namespace BlogService.Models.DTOs;

public class ReportDto
{
    public Guid Id { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReportedUserId { get; set; } = string.Empty;
    public string? ReportedUserName { get; set; }
    public ReportContentType ContentType { get; set; }
    public Guid ContentId { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? AdminNotes { get; set; }
    public bool UserBanned { get; set; }
    public bool ContentRemoved { get; set; }
    
    // Content details for display
    public string? ContentTitle { get; set; }
    public string? ContentExcerpt { get; set; }
}
