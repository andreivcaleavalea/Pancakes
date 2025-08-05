namespace AdminService.Clients.BlogClient.DTOs;

public class BlogPostDTO
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? FeaturedImage { get; set; }
    public int Status { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorImage { get; set; } = string.Empty; 
}