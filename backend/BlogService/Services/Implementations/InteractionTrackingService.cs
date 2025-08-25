using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

/// <summary>
/// Service to track user interactions and update their interests automatically
/// This should be called whenever users interact with blog posts
/// </summary>
public class InteractionTrackingService
{
    private readonly IUserInterestService _userInterestService;
    private readonly ILogger<InteractionTrackingService> _logger;

    public InteractionTrackingService(IUserInterestService userInterestService, ILogger<InteractionTrackingService> logger)
    {
        _userInterestService = userInterestService;
        _logger = logger;
    }

    /// <summary>
    /// Track user viewing a blog post
    /// </summary>
    public async Task TrackViewAsync(string userId, List<string> tags)
    {
        try
        {
            await _userInterestService.RecordInteractionAsync(userId, tags, "view");
            _logger.LogDebug("Tracked view interaction for user {UserId} on tags: {Tags}", userId, string.Join(", ", tags));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track view interaction for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Track user saving a blog post
    /// </summary>
    public async Task TrackSaveAsync(string userId, List<string> tags)
    {
        try
        {
            await _userInterestService.RecordInteractionAsync(userId, tags, "save");
            _logger.LogDebug("Tracked save interaction for user {UserId} on tags: {Tags}", userId, string.Join(", ", tags));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track save interaction for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Track user rating a blog post
    /// </summary>
    public async Task TrackRatingAsync(string userId, List<string> tags, double rating)
    {
        try
        {
            await _userInterestService.RecordInteractionAsync(userId, tags, "rate", rating);
            _logger.LogDebug("Tracked rating interaction for user {UserId} on tags: {Tags} with rating: {Rating}", 
                userId, string.Join(", ", tags), rating);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track rating interaction for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Track user commenting on a blog post
    /// </summary>
    public async Task TrackCommentAsync(string userId, List<string> tags)
    {
        try
        {
            await _userInterestService.RecordInteractionAsync(userId, tags, "comment");
            _logger.LogDebug("Tracked comment interaction for user {UserId} on tags: {Tags}", userId, string.Join(", ", tags));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track comment interaction for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Track user sharing a blog post
    /// </summary>
    public async Task TrackShareAsync(string userId, List<string> tags)
    {
        try
        {
            await _userInterestService.RecordInteractionAsync(userId, tags, "share");
            _logger.LogDebug("Tracked share interaction for user {UserId} on tags: {Tags}", userId, string.Join(", ", tags));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track share interaction for user {UserId}", userId);
        }
    }
}
