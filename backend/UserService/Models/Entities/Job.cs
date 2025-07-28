using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.Entities
{
    public class Job
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 255 characters")]
        public string Company { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Position is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Position must be between 2 and 255 characters")]
        public string Position { get; set; } = string.Empty;
        
        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters")]
        public string Location { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Start date is required")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "Start date must be in YYYY-MM format")]
        public string StartDate { get; set; } = string.Empty; // Format: YYYY-MM
        
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "End date must be in YYYY-MM format")]
        public string? EndDate { get; set; } // Format: YYYY-MM, nullable for current job
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
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