namespace UserService.Configuration;

/// <summary>
/// 🚀 Cache configuration for UserService performance optimization
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

    // Cache key prefixes
    public static class Keys
    {
        public const string UserById = "user_by_id_{0}";
        public const string UserByEmail = "user_by_email_{0}";
        public const string UserBatch = "users_batch_{0}";        // Hash of IDs
        public const string UsersByPage = "users_page_{0}_{1}";   // page, pageSize
        public const string UserFriends = "user_friends_{0}";     // userId
        public const string FriendshipStatus = "friendship_{0}_{1}"; // userId1, userId2
    }

    /// <summary>
    /// Create cache key with formatted parameters
    /// </summary>
    public static string FormatKey(string keyTemplate, params object[] args)
    {
        return string.Format(keyTemplate, args);
    }

    /// <summary>
    /// Create hash for ID collections
    /// </summary>
    public static string CreateHash(IEnumerable<string> ids)
    {
        var sortedIds = ids.OrderBy(x => x).ToArray();
        var combined = string.Join("_", sortedIds);
        return combined.GetHashCode().ToString("X");
    }
}