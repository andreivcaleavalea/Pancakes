namespace BlogService.Models.DTOs;

public class CommentLikeDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsLike { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentLikeDto
{
    public Guid CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsLike { get; set; } // true = like, false = dislike
}

public class CommentLikeStatsDto
{
    public Guid CommentId { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public bool? UserLike { get; set; } // null = no vote, true = liked, false = disliked
} 