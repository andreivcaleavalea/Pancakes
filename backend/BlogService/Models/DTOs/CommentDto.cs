namespace BlogService.Models.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorImage { get; set; } = string.Empty;
    public Guid BlogPostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 