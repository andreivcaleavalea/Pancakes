using BlogService.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.DTOs;

public class CreateBlogPostDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? FeaturedImage { get; set; }
    
    public PostStatus Status { get; set; } = PostStatus.Draft;
    
    [Required]
    public Guid AuthorId { get; set; }
    
    public DateTime? PublishedAt { get; set; }
}
