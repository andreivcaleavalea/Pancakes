using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string AuthorName { get; set; } = string.Empty;
    
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    [Required]
    public Guid BlogPostId { get; set; }
    
    public BlogPost BlogPost { get; set; } = null!;
    
    // For nested comments (replies)
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
} 