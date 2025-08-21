using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using BlogService.Configuration;

namespace BlogService.Services.Implementations;

public class RecommendationService : IRecommendationService
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IPostRatingRepository _postRatingRepository;
    private readonly ISavedBlogRepository _savedBlogRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RecommendationService> _logger;
    
    // Algorithm parameters
    private const int MinimumPostsForAlgorithm = 20;
    private const double FriendLikeWeight = 0.25;
    private const double UserSavedTagWeight = 0.20;
    private const double ViewCountWeight = 0.15;
    private const double AverageRatingWeight = 0.20;
    private const double RecencyWeight = 0.10;
    private const double TotalRatingsWeight = 0.10;

    public RecommendationService(
        IBlogPostRepository blogPostRepository,
        IPostRatingRepository postRatingRepository,
        ISavedBlogRepository savedBlogRepository,
        IFriendshipRepository friendshipRepository,
        IMemoryCache cache,
        ILogger<RecommendationService> logger)
    {
        _blogPostRepository = blogPostRepository;
        _postRatingRepository = postRatingRepository;
        _savedBlogRepository = savedBlogRepository;
        _friendshipRepository = friendshipRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<BlogPost>> GetPersonalizedRecommendationsAsync(string userId, int count = 5, string? excludeAuthorId = null)
    {
        try
        {
            var cacheKey = CacheConfig.FormatKey("recommendations", userId, count);
            if (_cache.TryGetValue(cacheKey, out IEnumerable<BlogPost>? cachedRecommendations))
            {
                return cachedRecommendations;
            }

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
            var userFriends = await _friendshipRepository.GetUserFriendsAsync(userId);

            // Extract user preferences from saved posts and ratings
            var preferredTags = GetUserPreferredTags(userSavedPosts, userRatings);
            
            // Get all published posts excluding user's own posts
            var (allPosts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = 1000, 
                Page = 1,
                ExcludeAuthorId = excludeAuthorId
            });

            // Calculate recommendation scores
            var recommendations = await CalculateRecommendationScores(
                allPosts, userId, userFriends, preferredTags, userSavedPosts, userRatings);

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

            // Cache results for 10 minutes
            _cache.Set(cacheKey, topRecommendations, TimeSpan.FromMinutes(10));

            _logger.LogInformation("Generated {Count} personalized recommendations for user {UserId}", 
                topRecommendations.Count, userId);

            return topRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating personalized recommendations for user {UserId}", userId);
            // Fallback to trending recommendations
            return await GetTrendingRecommendationsAsync(count, excludeAuthorId);
        }
    }

    public async Task<IEnumerable<BlogPost>> GetTrendingRecommendationsAsync(int count = 5, string? excludeAuthorId = null, IEnumerable<Guid>? excludePostIds = null)
    {
        try
        {
            var cacheKey = CacheConfig.FormatKey("trending", count, excludeAuthorId ?? "null");
            if (_cache.TryGetValue(cacheKey, out IEnumerable<BlogPost>? cachedTrending))
            {
                var filtered = excludePostIds != null 
                    ? cachedTrending.Where(p => !excludePostIds.Contains(p.Id))
                    : cachedTrending;
                return filtered.Take(count);
            }

            // Get posts with engagement metrics
            var (posts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = 500, 
                Page = 1,
                ExcludeAuthorId = excludeAuthorId
            });

            var trendingPosts = new List<(BlogPost Post, double Score)>();

            foreach (var post in posts)
            {
                if (excludePostIds?.Contains(post.Id) == true)
                    continue;

                var score = await CalculateTrendingScore(post);
                trendingPosts.Add((post, score));
            }

            var result = trendingPosts
                .OrderByDescending(tp => tp.Score)
                .Take(count)
                .Select(tp => tp.Post)
                .ToList();

            // Cache for 15 minutes
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating trending recommendations");
            return await GetSimplePopularRecommendationsAsync(count, excludeAuthorId);
        }
    }

    public async Task<IEnumerable<BlogPost>> GetSimplePopularRecommendationsAsync(int count = 5, string? excludeAuthorId = null)
    {
        try
        {
            // Simple fallback: most recent posts with highest engagement
            var (posts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = Math.Min(count * 3, 100), 
                Page = 1,
                SortBy = "createdAt",
                SortOrder = "desc",
                ExcludeAuthorId = excludeAuthorId
            });

            return posts.Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating simple popular recommendations");
            return Enumerable.Empty<BlogPost>();
        }
    }

    private async Task<List<(BlogPost Post, double Score)>> CalculateRecommendationScores(
        IEnumerable<BlogPost> posts, 
        string userId,
        IEnumerable<string> userFriends,
        Dictionary<string, double> preferredTags,
        IEnumerable<SavedBlog> userSavedPosts,
        IEnumerable<PostRating> userRatings)
    {
        var recommendations = new List<(BlogPost Post, double Score)>();
        var friendsList = userFriends.ToList();
        var savedPostIds = userSavedPosts.Select(sp => sp.BlogPostId).ToHashSet();
        var ratedPostIds = userRatings.Select(ur => ur.BlogPostId).ToHashSet();

        foreach (var post in posts)
        {
            // Skip posts user has already interacted with
            if (savedPostIds.Contains(post.Id) || ratedPostIds.Contains(post.Id))
                continue;

            double score = 0;

            // 1. Friend activity weight (25%)
            if (friendsList.Any())
            {
                var friendEngagement = await CalculateFriendEngagement(post.Id, friendsList);
                score += friendEngagement * FriendLikeWeight;
            }

            // 2. Tag preference weight (20%)
            if (preferredTags.Any())
            {
                var tagScore = CalculateTagScore(post.Tags, preferredTags);
                score += tagScore * UserSavedTagWeight;
            }

            // 3. View count weight (15%)
            var normalizedViewCount = NormalizeViewCount(post.ViewCount);
            score += normalizedViewCount * ViewCountWeight;

            // 4. Average rating weight (20%)
            var avgRating = await _postRatingRepository.GetAverageRatingAsync(post.Id);
            var normalizedRating = (double)avgRating / 5.0; // Normalize to 0-1
            score += normalizedRating * AverageRatingWeight;

            // 5. Recency weight (10%)
            var recencyScore = CalculateRecencyScore(post.PublishedAt ?? post.CreatedAt);
            score += recencyScore * RecencyWeight;

            // 6. Total ratings weight (10%) - indicates engagement level
            var totalRatings = await _postRatingRepository.GetTotalRatingsAsync(post.Id);
            var normalizedTotalRatings = Math.Min(totalRatings / 50.0, 1.0); // Normalize assuming 50+ ratings is excellent
            score += normalizedTotalRatings * TotalRatingsWeight;

            recommendations.Add((post, score));
        }

        return recommendations;
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

    private async Task<double> CalculateFriendEngagement(Guid postId, List<string> friendIds)
    {
        // Check if friends rated this post highly or saved it
        var friendRatings = await _postRatingRepository.GetPostRatingsByUsersAsync(postId, friendIds);
        var friendSaves = await _savedBlogRepository.GetPostSavesByUsersAsync(postId, friendIds);

        double score = 0;

        // High ratings from friends
        var highRatings = friendRatings.Where(fr => fr.Rating >= 4.0m).Count();
        score += highRatings * 0.3;

        // Friends who saved the post
        score += friendSaves.Count() * 0.5;

        // Normalize by number of friends
        return Math.Min(score / Math.Max(friendIds.Count, 1), 1.0);
    }

    private Dictionary<string, double> GetUserPreferredTags(IEnumerable<SavedBlog> savedPosts, IEnumerable<PostRating> ratings)
    {
        var tagScores = new Dictionary<string, double>();

        // Get tags from saved posts (higher weight)
        foreach (var savedPost in savedPosts)
        {
            foreach (var tag in savedPost.BlogPost?.Tags ?? new List<string>())
            {
                tagScores[tag] = tagScores.GetValueOrDefault(tag, 0) + 1.0;
            }
        }

        // Get tags from highly rated posts (4+ stars)
        var highRatings = ratings.Where(r => r.Rating >= 4.0m);
        foreach (var rating in highRatings)
        {
            foreach (var tag in rating.BlogPost?.Tags ?? new List<string>())
            {
                tagScores[tag] = tagScores.GetValueOrDefault(tag, 0) + 0.7;
            }
        }

        // Normalize scores
        var maxScore = tagScores.Values.DefaultIfEmpty(1).Max();
        return tagScores.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / maxScore);
    }

    private double CalculateTagScore(List<string> postTags, Dictionary<string, double> preferredTags)
    {
        if (!postTags.Any() || !preferredTags.Any())
            return 0;

        var matchingTags = postTags.Where(tag => preferredTags.ContainsKey(tag));
        return matchingTags.Sum(tag => preferredTags[tag]) / postTags.Count;
    }

    private double NormalizeViewCount(int viewCount)
    {
        // Logarithmic normalization to prevent very popular posts from dominating
        return Math.Log10(Math.Max(viewCount, 1) + 1) / Math.Log10(1001); // Assumes max ~1000 views
    }

    private double CalculateRecencyScore(DateTime publishedDate)
    {
        var daysSincePublished = (DateTime.UtcNow - publishedDate).TotalDays;
        
        // Exponential decay: newer posts get higher scores
        // Posts from today = 1.0, 7 days ago = ~0.5, 30 days ago = ~0.1
        return Math.Exp(-daysSincePublished / 14.0);
    }
}
