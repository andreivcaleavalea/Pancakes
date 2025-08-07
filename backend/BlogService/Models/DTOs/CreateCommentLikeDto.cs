using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.DTOs;

public class CreateCommentLikeDto
{
    [Required]
    public Guid CommentId { get; set; }
    
    // UserId is optional in request - it gets set from HttpContext in the service
    public string? UserId { get; set; }
    
    [Required]
    public bool IsLike { get; set; } // true = like, false = dislike
}
