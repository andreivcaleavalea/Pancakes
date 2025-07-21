using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Entities;

public class PersonalPageSettings
{
    [Key]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; } = false;
    
    [MaxLength(100)]
    public string? PageSlug { get; set; } // For custom URL like /profile/personal/john-doe
    
    // Section order (JSON array of section names)
    [MaxLength(500)]
    public string SectionOrder { get; set; } = "[\"personal\",\"education\",\"jobs\",\"projects\",\"hobbies\"]";
    
    // Section visibility (JSON object with section: boolean)
    [MaxLength(200)]
    public string SectionVisibility { get; set; } = "{\"personal\":true,\"education\":true,\"jobs\":true,\"projects\":true,\"hobbies\":true}";
    
    // Section templates (JSON object with section: template)
    [MaxLength(300)]
    public string SectionTemplates { get; set; } = "{\"personal\":\"card\",\"education\":\"timeline\",\"jobs\":\"timeline\",\"projects\":\"grid\",\"hobbies\":\"tags\"}";
    
    // Page theme/style
    [MaxLength(50)]
    public string Theme { get; set; } = "modern";
    
    // Color scheme
    [MaxLength(20)]
    public string ColorScheme { get; set; } = "blue";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
} 