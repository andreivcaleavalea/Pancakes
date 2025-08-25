using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using BlogService.Configuration;

namespace BlogService.Services.Implementations;

/// <summary>
/// Advanced recommendation service with user interest tracking and social signals
/// This service provides personalized content recommendations using multiple signals
/// </summary>
public class RecommendationService : IRecommendationService
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IPostRatingRepository _postRatingRepository;
    private readonly ISavedBlogRepository _savedBlogRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUserInterestService _userInterestService;
    private readonly IPersonalizedFeedRepository _personalizedFeedRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RecommendationService> _logger;
    private readonly RecommendationConfig _config;

    public RecommendationService(
        IBlogPostRepository blogPostRepository,
        IPostRatingRepository postRatingRepository,
        ISavedBlogRepository savedBlogRepository,
        IUserServiceClient userServiceClient,
        IUserInterestService userInterestService,
        IPersonalizedFeedRepository personalizedFeedRepository,
        IMemoryCache cache,
        ILogger<RecommendationService> logger,
        RecommendationConfig config)
    {
        _blogPostRepository = blogPostRepository;
        _postRatingRepository = postRatingRepository;
        _savedBlogRepository = savedBlogRepository;
        _userServiceClient = userServiceClient;
        _userInterestService = userInterestService;
        _personalizedFeedRepository = personalizedFeedRepository;
        _cache = cache;
        _logger = logger;
        _config = config;

        // Validate configuration on startup
        if (!_config.ValidateWeights())
        {
            throw new InvalidOperationException("Recommendation algorithm weights must sum to 1.0");
        }
    }

    public async Task<IEnumerable<BlogPost>> GetPersonalizedRecommendationsAsync(string userId, int count = 5, string? excludeAuthorId = null)
    {
        try
        {
            _logger.LogInformation("🤖 [RecommendationService] GetPersonalizedRecommendationsAsync called for user: {UserId}, count: {Count}", userId, count);

            // First try to get from pre-computed feed
            var personalizedFeed = await _personalizedFeedRepository.GetUserFeedAsync(userId);
            if (personalizedFeed?.IsValid == true)
            {
                _logger.LogInformation("⚡ [RecommendationService] Serving recommendations from PRE-COMPUTED feed for user {UserId}", userId);
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
            _logger.LogInformation("🔄 [RecommendationService] No valid pre-computed feed found - computing REAL-TIME recommendations for user {UserId}", userId);
            return await ComputePersonalizedRecommendationsAsync(userId, count, excludeAuthorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [RecommendationService] Error getting personalized recommendations for user {UserId} - falling back to SIMPLE POPULAR", userId);
            return await GetSimplePopularRecommendationsAsync(count, excludeAuthorId);
        }
    }

    /// <summary>
    /// Compute personalized recommendations in real-time with auth token for social signals
    /// </summary>
    public async Task<IEnumerable<BlogPost>> ComputePersonalizedRecommendationsWithTokenAsync(string userId, int count = -1, string? excludeAuthorId = null, string? authToken = null)
    {
        try
        {
            // Use default count if not specified
            if (count == -1) count = _config.DefaultRecommendationCount;
            
            // Check if we have enough posts for the algorithm
            var totalPostCount = await _blogPostRepository.GetTotalPublishedCountAsync();
            if (totalPostCount < _config.MinimumPostsForAlgorithm)
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
            
            // Get all published posts excluding user's own posts (limit for real-time performance)
            var (allPosts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = _config.RealtimeComputationLimit, 
                Page = 1,
                Status = PostStatus.Published,
                ExcludeAuthorId = excludeAuthorId,
                SortBy = "CreatedAt", // Get most recent posts first for better relevance
                SortOrder = "desc"
            });

            // Calculate recommendation scores with optimized batch processing
            var recommendations = await CalculateOptimizedRecommendationScores(
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
    public async Task<IEnumerable<BlogPost>> ComputePersonalizedRecommendationsAsync(string userId, int count = -1, string? excludeAuthorId = null)
    {
        try
        {
            // Use default count if not specified
            if (count == -1) count = _config.DefaultRecommendationCount;
            
            // Check if we have enough posts for the algorithm
            var totalPostCount = await _blogPostRepository.GetTotalPublishedCountAsync();
            if (totalPostCount < _config.MinimumPostsForAlgorithm)
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
            
            // Get all published posts excluding user's own posts (use full limit for background processing)
            var (allPosts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = _config.MaxPostsToFetch, 
                Page = 1,
                Status = PostStatus.Published,
                ExcludeAuthorId = excludeAuthorId,
                SortBy = "CreatedAt", // Get most recent posts first for better relevance
                SortOrder = "desc"
            });

            // Calculate recommendation scores with optimized batch processing
            var recommendations = await CalculateOptimizedRecommendationScores(
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
            var recommendations = await ComputePersonalizedRecommendationsAsync(userId, _config.PreComputationRecommendationCount); // Compute more for variety
            var blogPostIds = recommendations.Select(p => p.Id).ToList();
            
            // Create dummy scores for storage (real scores computed real-time)
            var scores = blogPostIds.Select((_, index) => 1.0 - (index * _config.FeedScoreDecrement)).ToList();
            
            await _personalizedFeedRepository.UpsertUserFeedAsync(userId, blogPostIds, scores, _config.FeedVersion);
            
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
            foreach (var friendId in friendIds.Take(_config.MaxFriendsToProcess)) // Limit to avoid performance issues
            {
                try
                {
                    var friendSaves = await _savedBlogRepository.GetUserSavedPostsAsync(friendId);
                    var recentSaves = friendSaves.Where(s => s.SavedAt > DateTime.UtcNow.AddDays(-_config.RecentActivityDays)).ToList();
                    
                    foreach (var save in recentSaves)
                    {
                        var postTags = save.BlogPost?.Tags ?? new List<string>();
                        foreach (var tag in postTags)
                        {
                            socialSignals[tag] = socialSignals.GetValueOrDefault(tag, 0) + _config.FriendSaveSignalWeight; // Friend saved
                        }
                    }

                    // Also consider friends' high ratings (4+ stars)
                    var friendRatings = await _postRatingRepository.GetUserRatingsAsync(friendId);
                    var highRatings = friendRatings.Where(r => r.Rating >= _config.HighRatingThreshold && r.CreatedAt > DateTime.UtcNow.AddDays(-_config.RecentActivityDays));
                    
                    foreach (var rating in highRatings)
                    {
                        var postTags = rating.BlogPost?.Tags ?? new List<string>();
                        foreach (var tag in postTags)
                        {
                            socialSignals[tag] = socialSignals.GetValueOrDefault(tag, 0) + _config.FriendRatingSignalWeight; // Friend rated highly
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

    /// <summary>
    /// Legacy method - kept for backward compatibility but no longer used
    /// Use CalculateOptimizedRecommendationScores for better performance
    /// </summary>
    [Obsolete("Use CalculateOptimizedRecommendationScores for better performance")]
    private async Task<List<(BlogPost Post, double Score)>> CalculateEnhancedRecommendationScores(
        IEnumerable<BlogPost> posts,
        string userId,
        Dictionary<string, double> userInterests,
        Dictionary<string, double> socialSignals,
        IEnumerable<SavedBlog> userSavedPosts,
        IEnumerable<PostRating> userRatings)
    {
        // Redirect to optimized version
        return await CalculateOptimizedRecommendationScores(posts, userId, userInterests, socialSignals, userSavedPosts, userRatings);
    }

    /// <summary>
    /// Optimized recommendation scoring with batch processing to eliminate N+1 queries
    /// </summary>
    private async Task<List<(BlogPost Post, double Score)>> CalculateOptimizedRecommendationScores(
        IEnumerable<BlogPost> posts,
        string userId,
        Dictionary<string, double> userInterests,
        Dictionary<string, double> socialSignals,
        IEnumerable<SavedBlog> userSavedPosts,
        IEnumerable<PostRating> userRatings)
    {
        var startTime = DateTime.UtcNow;
        var postsList = posts.ToList();
        var recommendations = new List<(BlogPost Post, double Score)>();
        var savedPostIds = userSavedPosts.Select(sp => sp.BlogPostId).ToHashSet();
        var ratedPostIds = userRatings.Select(ur => ur.BlogPostId).ToHashSet();

        if (_config.EnablePerformanceLogging)
        {
            _logger.LogInformation("🚀 [RecommendationService] Starting optimized scoring for {PostCount} posts", postsList.Count);
        }

        // Filter out posts user has already interacted with
        var candidatePosts = postsList.Where(p => !savedPostIds.Contains(p.Id) && !ratedPostIds.Contains(p.Id)).ToList();
        
        if (!candidatePosts.Any())
        {
            if (_config.EnablePerformanceLogging)
            {
                _logger.LogInformation("⚠️ [RecommendationService] No candidate posts after filtering - user has interacted with all posts");
            }
            return recommendations;
        }

        // ✨ PERFORMANCE OPTIMIZATION: Batch fetch all rating data in a single query
        var postIds = candidatePosts.Select(p => p.Id).ToList();
        var ratingStats = await _postRatingRepository.GetRatingStatsBatchAsync(postIds);

        if (_config.EnablePerformanceLogging)
        {
            var ratingFetchTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("⚡ [RecommendationService] Batch rating fetch completed in {Time}ms for {PostCount} posts", 
                ratingFetchTime, candidatePosts.Count);
        }

        // Process posts in batches for memory efficiency
        var batchSize = _config.BatchProcessingSize;
        var processedCount = 0;

        for (int i = 0; i < candidatePosts.Count; i += batchSize)
        {
            var batch = candidatePosts.Skip(i).Take(batchSize);
            
            foreach (var post in batch)
            {
                double score = 0;

                // 1. User Interest Score (25%) - from persistent interest tracking
                if (userInterests.Any() && post.Tags.Any())
                {
                    var interestScore = CalculateInterestScore(post.Tags, userInterests);
                    score += interestScore * _config.UserInterestWeight;
                }

                // 2. Social Signal Score (20%) - from friends' activity
                if (socialSignals.Any() && post.Tags.Any())
                {
                    var socialScore = CalculateSocialScore(post.Tags, socialSignals);
                    score += socialScore * _config.SocialSignalWeight;
                }

                // 3. View count weight (15%)
                var normalizedViewCount = NormalizeViewCount(post.ViewCount);
                score += normalizedViewCount * _config.ViewCountWeight;

                // 4. Average rating weight (20%) - ✨ NO DB CALL - using batch data
                var ratingData = ratingStats.GetValueOrDefault(post.Id, (0m, 0));
                var normalizedRating = (double)ratingData.Item1 / _config.MaxRatingValue;
                score += normalizedRating * _config.AverageRatingWeight;

                // 5. Recency weight (12%)
                var recencyScore = CalculateRecencyScore(post.PublishedAt ?? post.CreatedAt);
                score += recencyScore * _config.RecencyWeight;

                // 6. Total ratings weight (8%) - ✨ NO DB CALL - using batch data
                var totalRatings = ratingData.Item2;
                var normalizedTotalRatings = Math.Min(totalRatings / _config.TotalRatingsNormalizationFactor, 1.0);
                score += normalizedTotalRatings * _config.TotalRatingsWeight;

                recommendations.Add((post, score));
                processedCount++;
            }

            // Log progress for large datasets
            if (_config.EnablePerformanceLogging && candidatePosts.Count > 100)
            {
                var progressTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogDebug("📊 [RecommendationService] Processed {ProcessedCount}/{TotalCount} posts in {Time}ms", 
                    processedCount, candidatePosts.Count, progressTime);
            }
        }

        if (_config.EnablePerformanceLogging)
        {
            var totalTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("✅ [RecommendationService] Optimized scoring completed in {Time}ms for {PostCount} posts. DB calls reduced from {OldCalls} to 1!", 
                totalTime, candidatePosts.Count, candidatePosts.Count * 2);
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
        return Math.Log10(Math.Max(viewCount, 1) + 1) / Math.Log10(_config.ViewCountNormalizationFactor);
    }

    private double CalculateRecencyScore(DateTime publishedDate)
    {
        var daysSincePublished = (DateTime.UtcNow - publishedDate).TotalDays;
        return Math.Exp(-daysSincePublished / _config.RecencyDecayFactor);
    }

    // Implement the remaining required methods from IRecommendationService
    public async Task<IEnumerable<BlogPost>> GetSimplePopularRecommendationsAsync(int count, string? excludeAuthorId = null)
    {
        var (posts, totalCount) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
        { 
            PageSize = count * _config.SimplePopularMultiplier,
            Page = 1,
            Status = PostStatus.Published,
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
            PageSize = count * 3, // Keep this multiplier as it's for trending variety
            Page = 1,
            Status = PostStatus.Published,
            ExcludeAuthorId = excludeAuthorId
        });

        if (excludePostIds?.Any() == true)
        {
            posts = posts.Where(p => !excludePostIds.Contains(p.Id));
        }

        // ✨ PERFORMANCE OPTIMIZATION: Batch calculate trending scores
        var trendingPosts = await CalculateTrendingScoresBatch(posts.ToList());

        return trendingPosts
            .OrderByDescending(t => t.Score)
            .Take(count)
            .Select(t => t.Post);
    }

    /// <summary>
    /// Legacy method - kept for backward compatibility but no longer used
    /// Use CalculateTrendingScoresBatch for better performance
    /// </summary>
    [Obsolete("Use CalculateTrendingScoresBatch for better performance")]
    private async Task<double> CalculateTrendingScore(BlogPost post)
    {
        var batch = await CalculateTrendingScoresBatch(new List<BlogPost> { post });
        return batch.FirstOrDefault().Score;
    }

    /// <summary>
    /// Optimized trending score calculation with batch processing to eliminate N+1 queries
    /// </summary>
    private async Task<List<(BlogPost Post, double Score)>> CalculateTrendingScoresBatch(List<BlogPost> posts)
    {
        if (!posts.Any()) return new List<(BlogPost Post, double Score)>();

        var startTime = DateTime.UtcNow;
        
        if (_config.EnablePerformanceLogging)
        {
            _logger.LogInformation("🚀 [RecommendationService] Starting optimized trending score calculation for {PostCount} posts", posts.Count);
        }

        // ✨ PERFORMANCE OPTIMIZATION: Batch fetch all rating data in a single query
        var postIds = posts.Select(p => p.Id).ToList();
        var ratingStats = await _postRatingRepository.GetRatingStatsBatchAsync(postIds);

        if (_config.EnablePerformanceLogging)
        {
            var ratingFetchTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("⚡ [RecommendationService] Batch trending rating fetch completed in {Time}ms for {PostCount} posts", 
                ratingFetchTime, posts.Count);
        }

        var results = new List<(BlogPost Post, double Score)>();

        foreach (var post in posts)
        {
            var viewScore = NormalizeViewCount(post.ViewCount) * _config.TrendingViewWeight;
            
            // ✨ NO DB CALL - using batch data
            var ratingData = ratingStats.GetValueOrDefault(post.Id, (0m, 0));
            var ratingScore = ((double)ratingData.Item1 / _config.MaxRatingValue) * _config.TrendingRatingWeight;
            
            // ✨ NO DB CALL - using batch data
            var totalRatings = ratingData.Item2;
            var engagementScore = Math.Min(totalRatings / _config.TrendingEngagementNormalizationFactor, 1.0) * _config.TrendingEngagementWeight;
            
            var recencyScore = CalculateRecencyScore(post.PublishedAt ?? post.CreatedAt) * _config.TrendingRecencyWeight;

            var totalScore = viewScore + ratingScore + engagementScore + recencyScore;
            results.Add((post, totalScore));
        }

        if (_config.EnablePerformanceLogging)
        {
            var totalTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("✅ [RecommendationService] Optimized trending scoring completed in {Time}ms for {PostCount} posts. DB calls reduced from {OldCalls} to 1!", 
                totalTime, posts.Count, posts.Count * 2);
        }

        return results;
    }
}
