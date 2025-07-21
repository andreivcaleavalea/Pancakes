using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.DTOs;

public class CreateCommentDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment content must be between 1 and 1000 characters")]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Author name must be between 1 and 100 characters")]
    public string AuthorName { get; set; } = "Author Name";
    
    [Required]
    public Guid BlogPostId { get; set; }
    
    // Optional: for replies to existing comments
    public Guid? ParentCommentId { get; set; }
} 