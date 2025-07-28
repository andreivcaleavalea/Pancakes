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
    public Dictionary<string, string> SectionColors { get; set; } = new();
    public Dictionary<string, AdvancedSectionSettings> SectionAdvancedSettings { get; set; } = new();
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
    public Dictionary<string, string>? SectionColors { get; set; }
    public Dictionary<string, AdvancedSectionSettings>? SectionAdvancedSettings { get; set; }
    
    [StringLength(50, ErrorMessage = "Theme cannot exceed 50 characters")]
    public string? Theme { get; set; }
    
    [StringLength(20, ErrorMessage = "Color scheme cannot exceed 20 characters")]
    public string? ColorScheme { get; set; }
}

public class AdvancedSectionSettings
{
    public LayoutSettings Layout { get; set; } = new();
    public BackgroundSettings Background { get; set; } = new();
    public TypographySettings Typography { get; set; } = new();
    public StylingSettings Styling { get; set; } = new();
}

public class LayoutSettings
{
    public bool Fullscreen { get; set; } = false;
    public object Margin { get; set; } = "16px"; // Can be string ("16px") or number (16)
}

public class BackgroundSettings
{
    public string Color { get; set; } = "";
    public string Pattern { get; set; } = "none"; // "none", "dots", "grid", "diagonal", "waves"
    public double Opacity { get; set; } = 1.0; // 0.0 to 1.0
}

public class TypographySettings
{
    public string FontSize { get; set; } = "medium"; // "small", "medium", "large", "xl"
    public string FontColor { get; set; } = "";
    public string FontWeight { get; set; } = "normal"; // "normal", "medium", "semibold", "bold"
}

public class StylingSettings
{
    public bool RoundCorners { get; set; } = true;
    public string BorderRadius { get; set; } = "8px"; // "4px", "8px", "12px", "16px", "24px"
    public bool Shadow { get; set; } = true;
    public string ShadowIntensity { get; set; } = "medium"; // "light", "medium", "strong"
    public BorderSettings Border { get; set; } = new();
}

public class BorderSettings
{
    public bool Enabled { get; set; } = false;
    public string Color { get; set; } = "#e5e7eb";
    public string Width { get; set; } = "1px"; // "1px", "2px", "3px"
    public string Style { get; set; } = "solid"; // "solid", "dashed", "dotted"
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