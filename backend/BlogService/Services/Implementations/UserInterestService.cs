using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class UserInterestService : IUserInterestService
{
    private readonly IUserInterestRepository _userInterestRepository;
    private readonly ILogger<UserInterestService> _logger;

    // Interaction weights - how much each action contributes to interest scores
    private static readonly Dictionary<string, double> InteractionWeights = new()
    {
        { "view", 0.1 },        // Viewing a post
        { "save", 0.8 },        // Saving a post (strong signal)
        { "rate", 0.6 },        // Rating a post
        { "comment", 0.4 },     // Commenting on a post
        { "share", 0.5 }        // Sharing a post
    };

    public UserInterestService(IUserInterestRepository userInterestRepository, ILogger<UserInterestService> logger)
    {
        _userInterestRepository = userInterestRepository;
        _logger = logger;
    }

    public async Task RecordInteractionAsync(string userId, List<string> tags, string interactionType, double? rating = null)
    {
        if (!tags.Any()) return;

        var baseWeight = InteractionWeights.GetValueOrDefault(interactionType.ToLower(), 0.1);
        var tagScoreIncrements = new Dictionary<string, double>();

        foreach (var tag in tags)
        {
            var score = baseWeight;

            // Adjust score based on rating if provided
            if (rating.HasValue && interactionType.ToLower() == "rate")
            {
                // Scale rating from 1-5 to 0.2-1.0 multiplier
                var ratingMultiplier = (rating.Value - 1) / 4.0 * 0.8 + 0.2;
                score *= ratingMultiplier;
            }

            tagScoreIncrements[tag] = score;
        }

        await _userInterestRepository.UpdateUserInterestsAsync(userId, tagScoreIncrements);
        
        _logger.LogDebug("Recorded {InteractionType} interaction for user {UserId} on tags: {Tags}", 
            interactionType, userId, string.Join(", ", tags));
    }

    public async Task<Dictionary<string, double>> GetUserInterestsAsync(string userId)
    {
        return await _userInterestRepository.GetTopUserInterestsAsync(userId, 50);
    }

    public async Task<double> GetUserSimilarityAsync(string userId1, string userId2)
    {
        var interests1 = await GetUserInterestsAsync(userId1);
        var interests2 = await GetUserInterestsAsync(userId2);

        if (!interests1.Any() || !interests2.Any()) return 0.0;

        // Calculate cosine similarity
        var commonTags = interests1.Keys.Intersect(interests2.Keys).ToList();
        if (!commonTags.Any()) return 0.0;

        var dotProduct = commonTags.Sum(tag => interests1[tag] * interests2[tag]);
        var magnitude1 = Math.Sqrt(interests1.Values.Sum(score => score * score));
        var magnitude2 = Math.Sqrt(interests2.Values.Sum(score => score * score));

        if (magnitude1 == 0 || magnitude2 == 0) return 0.0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    public async Task DecayInterestsAsync()
    {
        await _userInterestRepository.DecayAllInterestsAsync(0.98); // 2% daily decay
        _logger.LogInformation("Applied interest decay across all users");
    }

    public async Task CleanupLowScoreInterestsAsync()
    {
        await _userInterestRepository.CleanupLowScoreInterestsAsync(0.01);
        _logger.LogInformation("Cleaned up low-score interests");
    }

    public async Task<Dictionary<string, object>> GetInterestStatisticsAsync()
    {
        // This would need additional repository methods for comprehensive stats
        // For now, return basic info
        var allInterests = await _userInterestRepository.GetUserInterestsAsync(""); // This needs to be implemented for all users
        
        return new Dictionary<string, object>
        {
            { "totalInterests", allInterests.Count() },
            { "averageScore", allInterests.Any() ? allInterests.Average(i => i.Score) : 0.0 }
        };
    }
}
