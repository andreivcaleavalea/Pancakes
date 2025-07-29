namespace BlogService.Models.DTOs;

public class CommentLikeDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsLike { get; set; }
    public DateTime CreatedAt { get; set; }
} 