using System.ComponentModel.DataAnnotations;

namespace BlogService.Models.Entities;

/// <summary>
/// Tracks user interests and preferences over time for personalized recommendations
/// </summary>
public class UserInterest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Tag { get; set; } = string.Empty;
    
    /// <summary>
    /// Interest score - higher means more interested
    /// Decays over time and updated based on user actions
    /// </summary>
    public double Score { get; set; } = 0.0;
    
    /// <summary>
    /// Number of times user interacted with this tag
    /// </summary>
    public int InteractionCount { get; set; } = 0;
    
    /// <summary>
    /// Last time this interest was updated (for decay calculation)
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Composite index on UserId + Tag for efficient lookups
    public static void ConfigureEntity(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserInterest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Tag }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LastUpdated);
        });
    }
}
