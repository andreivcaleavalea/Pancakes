namespace BlogService.Models.DTOs;

public class CreateSavedBlogDto
{
    public Guid BlogPostId { get; set; }
    // UserId will be populated from JWT token in controller
}
