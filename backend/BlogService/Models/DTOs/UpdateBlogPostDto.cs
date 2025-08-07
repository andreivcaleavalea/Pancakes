using BlogService.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.DTOs;

public class UpdateBlogPostDto
{
    [MaxLength(200)]
    public string? Title { get; set; }
    
    public string? Content { get; set; }
    
    [MaxLength(500)]
    public string? FeaturedImage { get; set; }
    
    public PostStatus? Status { get; set; }
    
    public DateTime? PublishedAt { get; set; }
    
    // Tags
    public List<string>? Tags { get; set; }
}
