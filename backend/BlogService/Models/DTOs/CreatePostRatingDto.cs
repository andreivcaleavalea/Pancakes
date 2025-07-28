using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.DTOs;

public class CreatePostRatingDto
{
    [Required(ErrorMessage = "BlogPostId is required")]
    public Guid BlogPostId { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Rating is required")]
    [Range(0.5, 5.0, ErrorMessage = "Rating must be between 0.5 and 5.0")]
    public decimal Rating { get; set; } // 0.5 to 5.0 with 0.5 increments
}
