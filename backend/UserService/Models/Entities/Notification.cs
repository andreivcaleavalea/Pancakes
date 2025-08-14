using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Entities
{
    public class Notification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(255)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty; // "BLOG_REMOVED", "BLOG_STATUS_CHANGED", etc.
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? BlogTitle { get; set; }
        
        [MaxLength(50)]
        public string? BlogId { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Source { get; set; } = string.Empty; // "ADMIN_ACTION", "REPORT_RESOLVED", etc.
        
        [MaxLength(1000)]
        public string? AdditionalData { get; set; } // JSON for any extra data
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReadAt { get; set; }
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}

