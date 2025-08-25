using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using BlogService.Configuration;

namespace BlogService.Services.Implementations;

/// <summary>
/// Enhanced recommendation service with user interest tracking and social signals
/// This is the new algorithm with friend-based recommendations and persistent interest tracking
/// </summary>
public class EnhancedRecommendationService : IRecommendationService
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IPostRatingRepository _postRatingRepository;
    private readonly ISavedBlogRepository _savedBlogRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUserInterestService _userInterestService;
    private readonly IPersonalizedFeedRepository _personalizedFeedRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EnhancedRecommendationService> _logger;
    
    // Enhanced Algorithm Parameters (sum = 1.0)
    private const int MinimumPostsForAlgorithm = 5;
    private const double UserInterestWeight = 0.25;        // Persistent user interests
    private const double SocialSignalWeight = 0.20;       // Friends' activity
    private const double ViewCountWeight = 0.15;          // Popularity
    private const double AverageRatingWeight = 0.20;      // Quality
    private const double RecencyWeight = 0.12;            // Freshness
    private const double TotalRatingsWeight = 0.08;       // Engagement

    public EnhancedRecommendationService(
        IBlogPostRepository blogPostRepository,
        IPostRatingRepository postRatingRepository,
        ISavedBlogRepository savedBlogRepository,
        IUserServiceClient userServiceClient,
        IUserInterestService userInterestService,
        IPersonalizedFeedRepository personalizedFeedRepository,
        IMemoryCache cache,
        ILogger<EnhancedRecommendationService> logger)
    {
        _blogPostRepository = blogPostRepository;
        _postRatingRepository = postRatingRepository;
        _savedBlogRepository = savedBlogRepository;
        _userServiceClient = userServiceClient;
        _userInterestService = userInterestService;
        _personalizedFeedRepository = personalizedFeedRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<BlogPost>> GetPersonalizedRecommendationsAsync(string userId, int count = 5, string? excludeAuthorId = null)
    {
        try
        {
            _logger.LogInformation("🤖 [EnhancedRecommendationService] GetPersonalizedRecommendationsAsync called for user: {UserId}, count: {Count}", userId, count);

            // First try to get from pre-computed feed
            var personalizedFeed = await _personalizedFeedRepository.GetUserFeedAsync(userId);
            if (personalizedFeed?.IsValid == true)
            {
                _logger.LogInformation("⚡ [EnhancedRecommendationService] Serving recommendations from PRE-COMPUTED feed for user {UserId}", userId);
                var feedPostIds = personalizedFeed.GetTopRecommendations(count);
                var feedPosts = new List<BlogPost>();
                
                foreach (var postId in feedPostIds)
                {
                    var post = await _blogPostRepository.GetByIdAsync(postId);
                    if (post != null && (excludeAuthorId == null || post.AuthorId != excludeAuthorId))
                    {
                        feedPosts.Add(post);
                    }
                }
                
                if (feedPosts.Count >= count)
                {
                    return feedPosts.Take(count);
                }
            }

            // Fall back to real-time computation
            _logger.LogInformation("🔄 [EnhancedRecommendationService] No valid pre-computed feed found - computing REAL-TIME recommendations for user {UserId}", userId);
            return await ComputePersonalizedRecommendationsAsync(userId, count, excludeAuthorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [EnhancedRecommendationService] Error getting personalized recommendations for user {UserId} - falling back to SIMPLE POPULAR", userId);
            return await GetSimplePopularRecommendationsAsync(count, excludeAuthorId);
        }
    }

    /// <summary>
    /// Compute personalized recommendations in real-time with auth token for social signals
    /// </summary>
    public async Task<IEnumerable<BlogPost>> ComputePersonalizedRecommendationsWithTokenAsync(string userId, int count = 20, string? excludeAuthorId = null, string? authToken = null)
    {
        try
        {
            // Check if we have enough posts for the algorithm
            var totalPostCount = await _blogPostRepository.GetTotalPublishedCountAsync();
            if (totalPostCount < MinimumPostsForAlgorithm)
            {
                _logger.LogInformation("Insufficient posts ({PostCount}) for recommendation algorithm, using simple popular", totalPostCount);
                return await GetSimplePopularRecommendationsAsync(count, excludeAuthorId);
            }

            // Get user's interaction data
            var userSavedPosts = await _savedBlogRepository.GetUserSavedPostsAsync(userId);
            var userRatings = await _postRatingRepository.GetUserRatingsAsync(userId);
            
            // Get user's persistent interests
            var userInterests = await _userInterestService.GetUserInterestsAsync(userId);
            
            // Get user's friends and their activity with auth token
            var socialSignals = await GetSocialSignalsAsync(userId, authToken);
            
            // Get all published posts excluding user's own posts
            var (allPosts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = 1000, 
                Page = 1,
                ExcludeAuthorId = excludeAuthorId
            });

            // Calculate recommendation scores with enhanced algorithm
            var recommendations = await CalculateEnhancedRecommendationScores(
                allPosts, userId, userInterests, socialSignals, userSavedPosts, userRatings);

            // Get top recommendations
            var topRecommendations = recommendations
                .OrderByDescending(r => r.Score)
                .Take(count)
                .Select(r => r.Post)
                .ToList();

            // If we don't have enough personalized recommendations, fill with trending
            if (topRecommendations.Count < count)
            {
                var excludeIds = topRecommendations.Select(p => p.Id).ToList();
                var additionalNeeded = count - topRecommendations.Count;
                
                var trendingPosts = await GetTrendingRecommendationsAsync(
                    additionalNeeded, excludeAuthorId, excludeIds);
                
                topRecommendations.AddRange(trendingPosts);
            }

            return topRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing personalized recommendations with token for user {UserId}", userId);
            return await GetSimplePopularRecommendationsAsync(count, excludeAuthorId);
        }
    }

    /// <summary>
    /// Compute personalized recommendations in real-time (used by background service and fallback)
    /// </summary>
    public async Task<IEnumerable<BlogPost>> ComputePersonalizedRecommendationsAsync(string userId, int count = 20, string? excludeAuthorId = null)
    {
        try
        {
            // Check if we have enough posts for the algorithm
            var totalPostCount = await _blogPostRepository.GetTotalPublishedCountAsync();
            if (totalPostCount < MinimumPostsForAlgorithm)
            {
                _logger.LogInformation("Insufficient posts ({PostCount}) for recommendation algorithm, using simple popular", totalPostCount);
                return await GetSimplePopularRecommendationsAsync(count, excludeAuthorId);
            }

            // Get user's interaction data
            var userSavedPosts = await _savedBlogRepository.GetUserSavedPostsAsync(userId);
            var userRatings = await _postRatingRepository.GetUserRatingsAsync(userId);
            
            // Get user's persistent interests
            var userInterests = await _userInterestService.GetUserInterestsAsync(userId);
            
            // Get user's friends and their activity (requires auth token for UserService API)
            var socialSignals = await GetSocialSignalsAsync(userId, null); // No auth token in background service
            
            // Get all published posts excluding user's own posts
            var (allPosts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = 1000, 
                Page = 1,
                ExcludeAuthorId = excludeAuthorId
            });

            // Calculate recommendation scores with enhanced algorithm
            var recommendations = await CalculateEnhancedRecommendationScores(
                allPosts, userId, userInterests, socialSignals, userSavedPosts, userRatings);

            // Get top recommendations
            var topRecommendations = recommendations
                .OrderByDescending(r => r.Score)
                .Take(count)
                .Select(r => r.Post)
                .ToList();

            // If we don't have enough personalized recommendations, fill with trending
            if (topRecommendations.Count < count)
            {
                var excludeIds = topRecommendations.Select(p => p.Id).ToList();
                var additionalNeeded = count - topRecommendations.Count;
                
                var trendingPosts = await GetTrendingRecommendationsAsync(
                    additionalNeeded, excludeAuthorId, excludeIds);
                
                topRecommendations.AddRange(trendingPosts);
            }

            return topRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing personalized recommendations for user {UserId}", userId);
            return await GetSimplePopularRecommendationsAsync(count, excludeAuthorId);
        }
    }

    /// <summary>
    /// Pre-compute and store personalized feed for a user (called by background service)
    /// </summary>
    public async Task PreComputeUserFeedAsync(string userId)
    {
        try
        {
            var recommendations = await ComputePersonalizedRecommendationsAsync(userId, 50); // Compute more for variety
            var blogPostIds = recommendations.Select(p => p.Id).ToList();
            
            // Create dummy scores for storage (real scores computed real-time)
            var scores = blogPostIds.Select((_, index) => 1.0 - (index * 0.01)).ToList();
            
            await _personalizedFeedRepository.UpsertUserFeedAsync(userId, blogPostIds, scores, "2.0");
            
            _logger.LogDebug("Pre-computed feed for user {UserId} with {Count} recommendations", userId, blogPostIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pre-computing feed for user {UserId}", userId);
        }
    }

    private async Task<Dictionary<string, double>> GetSocialSignalsAsync(string userId, string? authToken = null)
    {
        var socialSignals = new Dictionary<string, double>();
        
        try
        {
            // Skip social signals if no auth token provided
            if (string.IsNullOrEmpty(authToken))
            {
                _logger.LogDebug("No auth token provided for social signals, skipping friend recommendations for user {UserId}", userId);
                return socialSignals;
            }

            // Get user's friends from UserService
            var friendDtos = await _userServiceClient.GetUserFriendsAsync(authToken);
            var friendIds = friendDtos.Select(f => f.UserId).ToList();
            
            if (!friendIds.Any())
            {
                _logger.LogDebug("No friends found for user {UserId}", userId);
                return socialSignals;
            }

            _logger.LogDebug("Found {FriendCount} friends for user {UserId}", friendIds.Count, userId);

            // Get friends' recent saved posts and high ratings
            foreach (var friendId in friendIds.Take(10)) // Limit to avoid performance issues
            {
                try
                {
                    var friendSaves = await _savedBlogRepository.GetUserSavedPostsAsync(friendId);
                    var recentSaves = friendSaves.Where(s => s.SavedAt > DateTime.UtcNow.AddDays(-7)).ToList();
                    
                    foreach (var save in recentSaves)
                    {
                        var postTags = save.BlogPost?.Tags ?? new List<string>();
                        foreach (var tag in postTags)
                        {
                            socialSignals[tag] = socialSignals.GetValueOrDefault(tag, 0) + 0.3; // Friend saved
                        }
                    }

                    // Also consider friends' high ratings (4+ stars)
                    var friendRatings = await _postRatingRepository.GetUserRatingsAsync(friendId);
                    var highRatings = friendRatings.Where(r => r.Rating >= 4.0m && r.CreatedAt > DateTime.UtcNow.AddDays(-7));
                    
                    foreach (var rating in highRatings)
                    {
                        var postTags = rating.BlogPost?.Tags ?? new List<string>();
                        foreach (var tag in postTags)
                        {
                            socialSignals[tag] = socialSignals.GetValueOrDefault(tag, 0) + 0.2; // Friend rated highly
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting social signals from friend {FriendId}", friendId);
                    // Continue with other friends
                }
            }

            // Normalize social signals
            if (socialSignals.Any())
            {
                var maxSignal = socialSignals.Values.Max();
                socialSignals = socialSignals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / maxSignal);
                
                _logger.LogDebug("Generated social signals for user {UserId}: {Signals}", userId, 
                    string.Join(", ", socialSignals.Take(5).Select(kvp => $"{kvp.Key}={kvp.Value:F2}")));
            }
            else
            {
                _logger.LogDebug("No social signals generated for user {UserId} - friends have no recent activity", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting social signals for user {UserId}", userId);
        }

        return socialSignals;
    }

    private async Task<List<(BlogPost Post, double Score)>> CalculateEnhancedRecommendationScores(
        IEnumerable<BlogPost> posts,
        string userId,
        Dictionary<string, double> userInterests,
        Dictionary<string, double> socialSignals,
        IEnumerable<SavedBlog> userSavedPosts,
        IEnumerable<PostRating> userRatings)
    {
        var recommendations = new List<(BlogPost Post, double Score)>();
        var savedPostIds = userSavedPosts.Select(sp => sp.BlogPostId).ToHashSet();
        var ratedPostIds = userRatings.Select(ur => ur.BlogPostId).ToHashSet();

        foreach (var post in posts)
        {
            // Skip posts user has already interacted with
            if (savedPostIds.Contains(post.Id) || ratedPostIds.Contains(post.Id))
                continue;

            double score = 0;

            // 1. User Interest Score (25%) - from persistent interest tracking
            if (userInterests.Any() && post.Tags.Any())
            {
                var interestScore = CalculateInterestScore(post.Tags, userInterests);
                score += interestScore * UserInterestWeight;
            }

            // 2. Social Signal Score (20%) - from friends' activity
            if (socialSignals.Any() && post.Tags.Any())
            {
                var socialScore = CalculateSocialScore(post.Tags, socialSignals);
                score += socialScore * SocialSignalWeight;
            }

            // 3. View count weight (15%)
            var normalizedViewCount = NormalizeViewCount(post.ViewCount);
            score += normalizedViewCount * ViewCountWeight;

            // 4. Average rating weight (20%)
            var avgRating = await _postRatingRepository.GetAverageRatingAsync(post.Id);
            var normalizedRating = (double)avgRating / 5.0;
            score += normalizedRating * AverageRatingWeight;

            // 5. Recency weight (12%)
            var recencyScore = CalculateRecencyScore(post.PublishedAt ?? post.CreatedAt);
            score += recencyScore * RecencyWeight;

            // 6. Total ratings weight (8%) - indicates engagement level
            var totalRatings = await _postRatingRepository.GetTotalRatingsAsync(post.Id);
            var normalizedTotalRatings = Math.Min(totalRatings / 50.0, 1.0);
            score += normalizedTotalRatings * TotalRatingsWeight;

            recommendations.Add((post, score));
        }

        return recommendations;
    }

    private double CalculateInterestScore(List<string> postTags, Dictionary<string, double> userInterests)
    {
        if (!postTags.Any() || !userInterests.Any())
            return 0;

        var matchingTags = postTags.Where(tag => userInterests.ContainsKey(tag));
        if (!matchingTags.Any())
            return 0;

        // Weighted average of matching tags
        return matchingTags.Sum(tag => userInterests[tag]) / postTags.Count;
    }

    private double CalculateSocialScore(List<string> postTags, Dictionary<string, double> socialSignals)
    {
        if (!postTags.Any() || !socialSignals.Any())
            return 0;

        var matchingTags = postTags.Where(tag => socialSignals.ContainsKey(tag));
        if (!matchingTags.Any())
            return 0;

        // Weighted average of social signals
        return matchingTags.Sum(tag => socialSignals[tag]) / postTags.Count;
    }

    // Reuse existing helper methods
    private double NormalizeViewCount(int viewCount)
    {
        return Math.Log10(Math.Max(viewCount, 1) + 1) / Math.Log10(1001);
    }

    private double CalculateRecencyScore(DateTime publishedDate)
    {
        var daysSincePublished = (DateTime.UtcNow - publishedDate).TotalDays;
        return Math.Exp(-daysSincePublished / 14.0);
    }

    // Implement the remaining required methods from IRecommendationService
    public async Task<IEnumerable<BlogPost>> GetSimplePopularRecommendationsAsync(int count, string? excludeAuthorId = null)
    {
        var (posts, totalCount) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
        { 
            PageSize = count * 2,
            Page = 1,
            ExcludeAuthorId = excludeAuthorId,
            SortBy = "ViewCount",
            SortOrder = "desc"
        });

        return posts.Take(count);
    }

    public async Task<IEnumerable<BlogPost>> GetTrendingRecommendationsAsync(int count = 5, string? excludeAuthorId = null, IEnumerable<Guid>? excludePostIds = null)
    {
        var (posts, totalCount) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
        { 
            PageSize = count * 3,
            Page = 1,
            ExcludeAuthorId = excludeAuthorId
        });

        if (excludePostIds?.Any() == true)
        {
            posts = posts.Where(p => !excludePostIds.Contains(p.Id));
        }

        var trendingPosts = new List<(BlogPost Post, double Score)>();
        
        foreach (var post in posts)
        {
            var trendingScore = await CalculateTrendingScore(post);
            trendingPosts.Add((post, trendingScore));
        }

        return trendingPosts
            .OrderByDescending(t => t.Score)
            .Take(count)
            .Select(t => t.Post);
    }

    private async Task<double> CalculateTrendingScore(BlogPost post)
    {
        var viewScore = NormalizeViewCount(post.ViewCount) * 0.3;
        var avgRating = await _postRatingRepository.GetAverageRatingAsync(post.Id);
        var ratingScore = ((double)avgRating / 5.0) * 0.3;
        var totalRatings = await _postRatingRepository.GetTotalRatingsAsync(post.Id);
        var engagementScore = Math.Min(totalRatings / 20.0, 1.0) * 0.2;
        var recencyScore = CalculateRecencyScore(post.PublishedAt ?? post.CreatedAt) * 0.2;

        return viewScore + ratingScore + engagementScore + recencyScore;
    }
}
