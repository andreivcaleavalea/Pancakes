using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.DTOs;

public class PostRatingDto
{
    public Guid Id { get; set; }
    public Guid BlogPostId { get; set; }
    public string UserIdentifier { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePostRatingDto
{
    [Required(ErrorMessage = "BlogPostId is required")]
    public Guid BlogPostId { get; set; }
    
    public string UserIdentifier { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Rating is required")]
    [Range(0.5, 5.0, ErrorMessage = "Rating must be between 0.5 and 5.0")]
    public decimal Rating { get; set; } // 0.5 to 5.0 with 0.5 increments
}

public class PostRatingStatsDto
{
    public Guid BlogPostId { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public decimal? UserRating { get; set; } // Current user's rating if any
    
    // Rating distribution for display
    public Dictionary<decimal, int> RatingDistribution { get; set; } = new();
} 