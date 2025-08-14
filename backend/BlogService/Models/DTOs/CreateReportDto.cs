using System.ComponentModel.DataAnnotations;
using BlogService.Models.Entities;

namespace BlogService.Models.DTOs;

public class CreateReportDto
{
    [Required]
    public ReportContentType ContentType { get; set; }
    
    [Required]
    public Guid ContentId { get; set; }
    
    [Required]
    public ReportReason Reason { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    // ReporterId will be set from the authenticated user context
}
