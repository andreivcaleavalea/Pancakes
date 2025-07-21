using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.Entities;

public class PostRating
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid BlogPostId { get; set; }
    
    public BlogPost BlogPost { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string UserIdentifier { get; set; } = string.Empty; // IP or session ID for now
    
    [Required]
    [Range(1.0, 5.0)]
    public decimal Rating { get; set; } // Supports values like 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
} 