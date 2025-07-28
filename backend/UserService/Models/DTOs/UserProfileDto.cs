using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTOs
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 255 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-\.]+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, and periods")]
        public string Name { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters")]
        public string Avatar { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string Bio { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[0-9][\d]{0,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date of birth must be in YYYY-MM-DD format")]
        public string DateOfBirth { get; set; } = string.Empty;
    }
    
    public class EducationDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Institution name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Institution name must be between 2 and 255 characters")]
        public string Institution { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Specialization must be between 2 and 255 characters")]
        public string Specialization { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Degree is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Degree must be between 2 and 100 characters")]
        public string Degree { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Start date is required")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "Start date must be in YYYY-MM format")]
        public string StartDate { get; set; } = string.Empty;
        
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "End date must be in YYYY-MM format")]
        public string? EndDate { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
    }
    
    public class JobDto
    {
        public string Id { get; set; } = string.Empty;
        
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
        public string StartDate { get; set; } = string.Empty;
        
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "End date must be in YYYY-MM format")]
        public string? EndDate { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
    }
    
    public class HobbyDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Hobby name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Hobby name must be between 2 and 255 characters")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Skill level is required")]
        [RegularExpression(@"^(Beginner|Intermediate|Advanced)$", ErrorMessage = "Level must be Beginner, Intermediate, or Advanced")]
        public string Level { get; set; } = string.Empty;
    }
    
    public class ProjectDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Project name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Project name must be between 2 and 255 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Project description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Technologies are required")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Technologies must be between 2 and 500 characters")]
        public string Technologies { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Project URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ProjectUrl { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "GitHub URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid GitHub URL")]
        public string GithubUrl { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Start date is required")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "Start date must be in YYYY-MM format")]
        public string StartDate { get; set; } = string.Empty;
        
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "End date must be in YYYY-MM format")]
        public string? EndDate { get; set; }
    }
    
    public class UpdateUserProfileDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 255 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-\.]+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, and periods")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string Bio { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[0-9][\d]{0,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date of birth must be in YYYY-MM-DD format")]
        public string DateOfBirth { get; set; } = string.Empty;
    }
    
    public class ProfileDataDto
    {
        public UserProfileDto User { get; set; } = new();
        public List<EducationDto> Educations { get; set; } = new();
        public List<JobDto> Jobs { get; set; } = new();
        public List<HobbyDto> Hobbies { get; set; } = new();
        public List<ProjectDto> Projects { get; set; } = new();
    }
} 