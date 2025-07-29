using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.Entities;

public class SavedBlog
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public Guid BlogPostId { get; set; }
    
    public BlogPost BlogPost { get; set; } = null!;
    
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}