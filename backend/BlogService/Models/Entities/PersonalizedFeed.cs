using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogService.Models.Entities;

/// <summary>
/// Pre-computed personalized feed for users
/// Updated by background service every 15-30 minutes
/// </summary>
public class PersonalizedFeed
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Ordered list of blog post IDs for this user's personalized feed
    /// Stored as JSON array for efficiency
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<Guid> BlogPostIds { get; set; } = new List<Guid>();
    
    /// <summary>
    /// Scores corresponding to each blog post (same order as BlogPostIds)
    /// Used for debugging and potential re-ranking
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<double> Scores { get; set; } = new List<double>();
    
    /// <summary>
    /// Algorithm version used to generate this feed
    /// Useful for A/B testing and rollbacks
    /// </summary>
    public string AlgorithmVersion { get; set; } = "1.0";
    
    /// <summary>
    /// When this feed was computed
    /// </summary>
    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this feed expires and needs recomputation
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Get top N recommendations from this feed
    /// </summary>
    public List<Guid> GetTopRecommendations(int count = 4)
    {
        return BlogPostIds.Take(count).ToList();
    }

    /// <summary>
    /// Check if this feed is still valid (not expired)
    /// </summary>
    public bool IsValid => DateTime.UtcNow < ExpiresAt;

    public static void ConfigureEntity(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonalizedFeed>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.ComputedAt);
        });
    }
}
