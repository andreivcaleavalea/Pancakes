using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Entities
{
    public class Ban
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(255)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
        
        public DateTime BannedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(255)]
        public string BannedBy { get; set; } = string.Empty;
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? UnbannedAt { get; set; }
        
        [MaxLength(255)]
        public string? UnbannedBy { get; set; }
        
        [MaxLength(500)]
        public string? UnbanReason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}