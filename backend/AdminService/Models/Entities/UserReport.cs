using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Entities
{
    public class UserReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(450)]
        public string ReportedUserId { get; set; } = string.Empty;
        
        [MaxLength(450)]
        public string? ReporterUserId { get; set; } // Nullable for anonymous reports
        
        [Required]
        [MaxLength(100)]
        public string Reason { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // pending, reviewed, dismissed, action_taken
        
        [MaxLength(450)]
        public string? ReviewedBy { get; set; }
        
        public DateTime? ReviewedAt { get; set; }
        
        [MaxLength(500)]
        public string? ReviewNotes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}