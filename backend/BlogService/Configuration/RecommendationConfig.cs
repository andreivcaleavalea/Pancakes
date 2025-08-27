namespace BlogService.Configuration;

/// <summary>
/// Configuration settings for the recommendation service
/// Contains all configurable parameters to avoid magic numbers
/// </summary>
public class RecommendationConfig
{
    /// <summary>
    /// Algorithm behavior settings
    /// </summary>
    public int MinimumPostsForAlgorithm { get; set; } = 5;
    public int MaxPostsToFetch { get; set; } = 1000;
    public int DefaultRecommendationCount { get; set; } = 20;
    public int PreComputationRecommendationCount { get; set; } = 50;
    
    /// <summary>
    /// Performance optimization settings
    /// </summary>
    public int RealtimeComputationLimit { get; set; } = 500; // Reduce from 1000 for better performance
    public int BatchProcessingSize { get; set; } = 100; // Process posts in batches for memory efficiency
    public bool EnablePerformanceLogging { get; set; } = true;

    /// <summary>
    /// Social signals settings
    /// </summary>
    public int MaxFriendsToProcess { get; set; } = 10;
    public int RecentActivityDays { get; set; } = 7;
    public decimal HighRatingThreshold { get; set; } = 4.0m;
    public double FriendSaveSignalWeight { get; set; } = 0.3;
    public double FriendRatingSignalWeight { get; set; } = 0.2;

    /// <summary>
    /// Score calculation settings
    /// </summary>
    public double MaxRatingValue { get; set; } = 5.0;
    public double TotalRatingsNormalizationFactor { get; set; } = 50.0;
    public double ViewCountNormalizationFactor { get; set; } = 1001.0;
    public double RecencyDecayFactor { get; set; } = 14.0;

    /// <summary>
    /// Simple popular recommendations settings
    /// </summary>
    public int SimplePopularMultiplier { get; set; } = 2;

    /// <summary>
    /// Trending score weights
    /// </summary>
    public double TrendingViewWeight { get; set; } = 0.3;
    public double TrendingRatingWeight { get; set; } = 0.3;
    public double TrendingEngagementWeight { get; set; } = 0.2;
    public double TrendingRecencyWeight { get; set; } = 0.2;
    public double TrendingEngagementNormalizationFactor { get; set; } = 20.0;

    /// <summary>
    /// Feed pre-computation settings
    /// </summary>
    public string FeedVersion { get; set; } = "2.0";
    public double FeedScoreDecrement { get; set; } = 0.01;

    /// <summary>
    /// Algorithm weight distribution (must sum to 1.0)
    /// </summary>
    public double UserInterestWeight { get; set; } = 0.25;
    public double SocialSignalWeight { get; set; } = 0.20;
    public double ViewCountWeight { get; set; } = 0.15;
    public double AverageRatingWeight { get; set; } = 0.20;
    public double RecencyWeight { get; set; } = 0.12;
    public double TotalRatingsWeight { get; set; } = 0.08;

    /// <summary>
    /// Validates that algorithm weights sum to 1.0
    /// </summary>
    public bool ValidateWeights()
    {
        var totalWeight = UserInterestWeight + SocialSignalWeight + ViewCountWeight + 
                         AverageRatingWeight + RecencyWeight + TotalRatingsWeight;
        return Math.Abs(totalWeight - 1.0) < 0.001; // Allow small floating point tolerance
    }
}
