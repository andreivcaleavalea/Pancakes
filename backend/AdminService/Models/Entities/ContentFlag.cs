using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Entities
{
    public class ContentFlag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(20)]
        public string ContentType { get; set; } = string.Empty; // blog_post, comment
        
        [Required]
        [MaxLength(450)]
        public string ContentId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string FlagType { get; set; } = string.Empty; // spam, inappropriate, copyright, etc.
        
        [MaxLength(450)]
        public string? FlaggedBy { get; set; } // Nullable for auto-detection
        
        public bool AutoDetected { get; set; } = false;
        
        public int Severity { get; set; } = 1; // 1-5 severity level
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // pending, reviewed, dismissed, action_taken
        
        [MaxLength(450)]
        public string? ReviewedBy { get; set; }
        
        public DateTime? ReviewedAt { get; set; }
        
        [MaxLength(500)]
        public string? ReviewNotes { get; set; }
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}