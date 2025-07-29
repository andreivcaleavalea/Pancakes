namespace BlogService.Models.DTOs;

public class CommentLikeStatsDto
{
    public Guid CommentId { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public bool? UserLike { get; set; } // null = no vote, true = liked, false = disliked
}
