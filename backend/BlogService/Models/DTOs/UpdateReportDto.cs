using System.ComponentModel.DataAnnotations;
using BlogService.Models.Entities;

namespace BlogService.Models.DTOs;

public class UpdateReportDto
{
    [Required]
    public ReportStatus Status { get; set; }
    
    [MaxLength(1000)]
    public string? AdminNotes { get; set; }
    
    public bool UserBanned { get; set; } = false;
    public bool ContentRemoved { get; set; } = false;
    
    // ReviewedBy will be set from the authenticated admin context
}
