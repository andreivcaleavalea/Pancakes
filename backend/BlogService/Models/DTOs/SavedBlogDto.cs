namespace BlogService.Models.DTOs;

public class SavedBlogDto
{
    public string UserId { get; set; } = string.Empty;
    public Guid BlogPostId { get; set; }
    public DateTime SavedAt { get; set; }
    public BlogPostDto? BlogPost { get; set; }
}

public class CreateSavedBlogDto
{
    public Guid BlogPostId { get; set; }
    // UserId will be populated from JWT token in controller
}