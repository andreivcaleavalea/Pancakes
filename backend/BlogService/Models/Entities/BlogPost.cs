using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.Entities;

public class BlogPost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? FeaturedImage { get; set; }
    
    public PostStatus Status { get; set; } = PostStatus.Draft;
    
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}
