using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Entities
{
    public class AdminRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public int Level { get; set; } = 1;
        
        [Required]
        public string Permissions { get; set; } = "[]"; // JSON array of permissions
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<AdminUser> AdminUsers { get; set; } = new List<AdminUser>();
    }
}