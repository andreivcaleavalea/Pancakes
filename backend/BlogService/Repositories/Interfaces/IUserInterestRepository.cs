using BlogService.Models.Entities;

namespace BlogService.Repositories.Interfaces;

public interface IUserInterestRepository
{
    /// <summary>
    /// Get all interests for a specific user
    /// </summary>
    Task<IEnumerable<UserInterest>> GetUserInterestsAsync(string userId);
    
    /// <summary>
    /// Get user's interest score for a specific tag
    /// </summary>
    Task<UserInterest?> GetUserInterestAsync(string userId, string tag);
    
    /// <summary>
    /// Update or create user interest for a tag
    /// </summary>
    Task<UserInterest> UpsertUserInterestAsync(string userId, string tag, double scoreIncrement);
    
    /// <summary>
    /// Batch update multiple interests for a user
    /// </summary>
    Task UpdateUserInterestsAsync(string userId, Dictionary<string, double> tagScoreIncrements);
    
    /// <summary>
    /// Decay all interest scores (called periodically to reduce old interests)
    /// </summary>
    Task DecayAllInterestsAsync(double decayFactor = 0.95);
    
    /// <summary>
    /// Get top interests for a user (highest scoring tags)
    /// </summary>
    Task<Dictionary<string, double>> GetTopUserInterestsAsync(string userId, int topCount = 20);
    
    /// <summary>
    /// Remove very low scoring interests (cleanup)
    /// </summary>
    Task CleanupLowScoreInterestsAsync(double minimumScore = 0.01);
}
