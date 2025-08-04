using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Entities
{
    public class AdminAuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(450)]
        public string AdminId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // USER_BANNED, POST_DELETED, etc.
        
        [Required]
        [MaxLength(50)]
        public string TargetType { get; set; } = string.Empty; // User, BlogPost, Comment, etc.
        
        [Required]
        [MaxLength(450)]
        public string TargetId { get; set; } = string.Empty;
        
        public string Details { get; set; } = string.Empty; // JSON with before/after states
        
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual AdminUser AdminUser { get; set; } = null!;
    }
}