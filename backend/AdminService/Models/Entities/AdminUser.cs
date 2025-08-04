using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Entities
{
    public class AdminUser
    {
        [Key]
        [MaxLength(450)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public int AdminLevel { get; set; } = 1; // 1-4 hierarchy
        
        public bool IsActive { get; set; } = true;
        public bool RequirePasswordChange { get; set; } = false;
        public bool TwoFactorEnabled { get; set; } = false;
        
        [MaxLength(450)]
        public string? CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
        public DateTime PasswordChangedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<AdminRole> Roles { get; set; } = new List<AdminRole>();
        public virtual ICollection<AdminAuditLog> AuditLogs { get; set; } = new List<AdminAuditLog>();
    }
}