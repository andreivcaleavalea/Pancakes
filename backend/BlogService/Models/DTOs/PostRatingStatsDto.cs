namespace BlogService.Models.DTOs;

public class PostRatingStatsDto
{
    public Guid BlogPostId { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public decimal? UserRating { get; set; } // Current user's rating if any
    
    // Rating distribution for display
    public Dictionary<decimal, int> RatingDistribution { get; set; } = new();
}
