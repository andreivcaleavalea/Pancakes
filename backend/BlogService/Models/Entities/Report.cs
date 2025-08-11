using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.Entities;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string ReporterId { get; set; } = string.Empty;
    
    [Required]
    public string ReporterName { get; set; } = string.Empty;
    
    [Required]
    public string ReportedUserId { get; set; } = string.Empty;
    
    public string? ReportedUserName { get; set; }
    
    [Required]
    public ReportContentType ContentType { get; set; }
    
    [Required]
    public Guid ContentId { get; set; }
    
    [Required]
    public ReportReason Reason { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Admin action fields
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    [MaxLength(1000)]
    public string? AdminNotes { get; set; }
    
    // Actions taken
    public bool UserBanned { get; set; } = false;
    public bool ContentRemoved { get; set; } = false;
}
