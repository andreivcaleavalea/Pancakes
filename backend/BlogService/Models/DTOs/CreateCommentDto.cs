using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.DTOs;

public class CreateCommentDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment content must be between 1 and 1000 characters")]
    public string Content { get; set; } = string.Empty;
    
    // AuthorName will be populated by the backend from UserService
    public string AuthorName { get; set; } = string.Empty;
    
    // AuthorId will be populated by the backend from JWT token
    public string AuthorId { get; set; } = string.Empty;
    
    [Required]
    public Guid BlogPostId { get; set; }
    
    // Optional: for replies to existing comments
    public Guid? ParentCommentId { get; set; }
} 