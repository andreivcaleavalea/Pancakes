using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.Entities
{
    public class Hobby
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "Hobby name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Hobby name must be between 2 and 255 characters")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Skill level is required")]
        [RegularExpression(@"^(Beginner|Intermediate|Advanced)$", ErrorMessage = "Level must be Beginner, Intermediate, or Advanced")]
        public string Level { get; set; } = string.Empty; // Beginner, Intermediate, Advanced
        
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