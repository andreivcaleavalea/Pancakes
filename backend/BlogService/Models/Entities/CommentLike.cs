using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.Entities;

public class CommentLike
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid CommentId { get; set; }
    
    public Comment Comment { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string UserIdentifier { get; set; } = string.Empty; // IP or session ID for now
    
    [Required]
    public bool IsLike { get; set; } // true = like, false = dislike
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
} 