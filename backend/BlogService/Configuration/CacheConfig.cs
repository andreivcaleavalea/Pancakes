namespace BlogService.Configuration;

/// <summary>
/// 🚀 Cache configuration for performance optimization
/// Centralizes cache keys and durations for consistent caching strategy
/// </summary>
public static class CacheConfig
{
    // Cache duration constants
    public static class Duration
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);     // 5 minutes - frequently changing data
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(15);   // 15 minutes - moderately stable data  
        public static readonly TimeSpan Long = TimeSpan.FromHours(1);        // 1 hour - stable data
        public static readonly TimeSpan VeryLong = TimeSpan.FromHours(4);    // 4 hours - very stable data
    }

    // Cache key prefixes for organized cache management
    public static class Keys
    {
        // User-related cache keys
        public const string UserById = "user_by_id_{0}";
        public const string UserBatch = "users_batch_{0}";        // Hash of IDs for batch lookup
        
        // Blog post cache keys
        public const string BlogPostById = "blog_post_{0}";
        public const string BlogPostsByPage = "blog_posts_page_{0}_{1}_{2}";  // page, pageSize, hash of filters
        public const string FeaturedPosts = "featured_posts_{0}";             // count
        public const string PopularPosts = "popular_posts_{0}";               // count
        public const string FriendsPosts = "friends_posts_{0}_{1}_{2}";       // hash of friendIds, page, pageSize
        
        // Tag cache keys
        public const string PopularTags = "popular_tags_{0}";      // limit
        public const string TagSearch = "tag_search_{0}_{1}";      // query, limit
        
        // Comment cache keys  
        public const string CommentsByPost = "comments_by_post_{0}_{1}_{2}";  // blogPostId, page, pageSize
        public const string CommentById = "comment_{0}";
        
        // Stats cache keys
        public const string PostRatingStats = "post_rating_stats_{0}";        // blogPostId
    }

    /// <summary>
    /// Create cache key with formatted parameters
    /// </summary>
    public static string FormatKey(string keyTemplate, params object[] args)
    {
        return string.Format(keyTemplate, args);
    }

    /// <summary>
    /// Create hash for complex objects (like filter parameters or ID lists)
    /// </summary>
    public static string CreateHash(params object[] objects)
    {
        var combined = string.Join("_", objects.Where(o => o != null).Select(o => o.ToString()));
        return combined.GetHashCode().ToString("X");
    }
}