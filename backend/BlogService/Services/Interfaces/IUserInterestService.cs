namespace BlogService.Services.Interfaces;

/// <summary>
/// Service for tracking and managing user interests based on their interactions
/// </summary>
public interface IUserInterestService
{
    /// <summary>
    /// Record user interaction with content (view, save, rate, etc.)
    /// This will update their interest scores for related tags
    /// </summary>
    Task RecordInteractionAsync(string userId, List<string> tags, string interactionType, double? rating = null);
    
    /// <summary>
    /// Get user's current interest scores for personalized recommendations
    /// </summary>
    Task<Dictionary<string, double>> GetUserInterestsAsync(string userId);
    
    /// <summary>
    /// Get similarity score between two users based on their interests
    /// Used for collaborative filtering
    /// </summary>
    Task<double> GetUserSimilarityAsync(string userId1, string userId2);
    
    /// <summary>
    /// Decay old interests (called by background service)
    /// </summary>
    Task DecayInterestsAsync();
    
    /// <summary>
    /// Clean up very low scoring interests
    /// </summary>
    Task CleanupLowScoreInterestsAsync();
    
    /// <summary>
    /// Get interest trends and statistics for monitoring
    /// </summary>
    Task<Dictionary<string, object>> GetInterestStatisticsAsync();
}
