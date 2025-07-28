using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserService.Models.Entities
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 255 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-\.]+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, and periods")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Image { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string ProviderUserId { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string Bio { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[0-9][\d]{0,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public DateTime? DateOfBirth { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Education> Educations { get; set; } = new List<Education>();
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
        public virtual ICollection<Hobby> Hobbies { get; set; } = new List<Hobby>();
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
} 