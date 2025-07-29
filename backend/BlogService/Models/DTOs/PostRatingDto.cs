namespace BlogService.Models.DTOs;

public class PostRatingDto
{
    public Guid Id { get; set; }
    public Guid BlogPostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public DateTime CreatedAt { get; set; }
} 