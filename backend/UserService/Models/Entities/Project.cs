using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.Entities
{
    public class Project
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "Project name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Project name must be between 2 and 255 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Project description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Technologies are required")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Technologies must be between 2 and 500 characters")]
        public string Technologies { get; set; } = string.Empty; // Comma-separated list
        
        [StringLength(500, ErrorMessage = "Project URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ProjectUrl { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "GitHub URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid GitHub URL")]
        public string GithubUrl { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Start date is required")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "Start date must be in YYYY-MM format")]
        public string StartDate { get; set; } = string.Empty; // Format: YYYY-MM
        
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "End date must be in YYYY-MM format")]
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