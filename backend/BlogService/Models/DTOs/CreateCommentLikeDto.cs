namespace BlogService.Models.DTOs;

public class CreateCommentLikeDto
{
    public Guid CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsLike { get; set; } // true = like, false = dislike
}
