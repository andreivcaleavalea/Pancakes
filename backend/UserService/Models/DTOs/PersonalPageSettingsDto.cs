using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTOs;

public class PersonalPageSettingsDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string? PageSlug { get; set; }
    public List<string> SectionOrder { get; set; } = new();
    public Dictionary<string, bool> SectionVisibility { get; set; } = new();
    public Dictionary<string, string> SectionTemplates { get; set; } = new();
    public string Theme { get; set; } = "modern";
    public string ColorScheme { get; set; } = "blue";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdatePersonalPageSettingsDto
{
    public bool? IsPublic { get; set; }
    
    [StringLength(100, ErrorMessage = "Page slug cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "Page slug can only contain letters, numbers, hyphens, and underscores")]
    public string? PageSlug { get; set; }
    
    public List<string>? SectionOrder { get; set; }
    public Dictionary<string, bool>? SectionVisibility { get; set; }
    public Dictionary<string, string>? SectionTemplates { get; set; }
    
    [StringLength(50, ErrorMessage = "Theme cannot exceed 50 characters")]
    public string? Theme { get; set; }
    
    [StringLength(20, ErrorMessage = "Color scheme cannot exceed 20 characters")]
    public string? ColorScheme { get; set; }
}

public class PublicPersonalPageDto
{
    public UserProfileDto User { get; set; } = new();
    public List<EducationDto> Educations { get; set; } = new();
    public List<JobDto> Jobs { get; set; } = new();
    public List<HobbyDto> Hobbies { get; set; } = new();
    public List<ProjectDto> Projects { get; set; } = new();
    public PersonalPageSettingsDto Settings { get; set; } = new();
} 