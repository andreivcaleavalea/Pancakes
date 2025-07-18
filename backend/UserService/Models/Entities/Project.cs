using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.Entities
{
    public class Project
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Technologies { get; set; } = string.Empty; // Comma-separated list
        
        [MaxLength(500)]
        public string ProjectUrl { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string GithubUrl { get; set; } = string.Empty;
        
        public string StartDate { get; set; } = string.Empty; // Format: YYYY-MM
        
        public string? EndDate { get; set; } // Format: YYYY-MM, nullable for ongoing
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
} 