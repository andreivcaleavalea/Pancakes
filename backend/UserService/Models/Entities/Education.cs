using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.Entities
{
    public class Education
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(255)]
        public string Institution { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Specialization { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Degree { get; set; } = string.Empty;
        
        [Required]
        public string StartDate { get; set; } = string.Empty; // Format: YYYY-MM
        
        public string? EndDate { get; set; } // Format: YYYY-MM, nullable for ongoing
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
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