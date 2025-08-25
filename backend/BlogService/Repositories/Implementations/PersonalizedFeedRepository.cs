using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class PersonalizedFeedRepository : IPersonalizedFeedRepository
{
    private readonly BlogDbContext _context;
    private readonly ILogger<PersonalizedFeedRepository> _logger;

    public PersonalizedFeedRepository(BlogDbContext context, ILogger<PersonalizedFeedRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PersonalizedFeed?> GetUserFeedAsync(string userId)
    {
        return await _context.PersonalizedFeeds
            .FirstOrDefaultAsync(pf => pf.UserId == userId);
    }

    public async Task<PersonalizedFeed> UpsertUserFeedAsync(string userId, List<Guid> blogPostIds, List<double> scores, string algorithmVersion = "1.0")
    {
        var existingFeed = await GetUserFeedAsync(userId);
        var now = DateTime.UtcNow;

        if (existingFeed != null)
        {
            existingFeed.BlogPostIds = blogPostIds;
            existingFeed.Scores = scores;
            existingFeed.AlgorithmVersion = algorithmVersion;
            existingFeed.ComputedAt = now;
            existingFeed.ExpiresAt = now.AddMinutes(30);
            existingFeed.UpdatedAt = now;
            
            await _context.SaveChangesAsync();
            return existingFeed;
        }
        else
        {
            var newFeed = new PersonalizedFeed
            {
                UserId = userId,
                BlogPostIds = blogPostIds,
                Scores = scores,
                AlgorithmVersion = algorithmVersion,
                ComputedAt = now,
                ExpiresAt = now.AddMinutes(30),
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.PersonalizedFeeds.Add(newFeed);
            await _context.SaveChangesAsync();
            return newFeed;
        }
    }

    public async Task<IEnumerable<PersonalizedFeed>> GetExpiredFeedsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.PersonalizedFeeds
            .Where(pf => pf.ExpiresAt <= now)
            .ToListAsync();
    }

    public async Task<IEnumerable<PersonalizedFeed>> GetExpiringFeedsAsync(int minutesUntilExpiry = 5)
    {
        var expiryThreshold = DateTime.UtcNow.AddMinutes(minutesUntilExpiry);
        return await _context.PersonalizedFeeds
            .Where(pf => pf.ExpiresAt <= expiryThreshold && pf.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetUsersWithoutFeedsAsync()
    {
        // This is a complex query - get users who have interacted with the system but don't have feeds
        // We'll use users who have saved posts or ratings as a proxy for active users
        var usersWithSaves = await _context.SavedBlogs
            .Select(sb => sb.UserId)
            .Distinct()
            .ToListAsync();

        var usersWithRatings = await _context.PostRatings
            .Select(pr => pr.UserId)
            .Distinct()
            .ToListAsync();

        var allActiveUsers = usersWithSaves.Union(usersWithRatings).ToList();

        var usersWithFeeds = await _context.PersonalizedFeeds
            .Select(pf => pf.UserId)
            .ToListAsync();

        return allActiveUsers.Except(usersWithFeeds).ToList();
    }

    public async Task DeleteExpiredFeedsAsync(int daysOld = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var expiredFeeds = await _context.PersonalizedFeeds
            .Where(pf => pf.ExpiresAt < cutoffDate)
            .ToListAsync();

        if (expiredFeeds.Any())
        {
            _context.PersonalizedFeeds.RemoveRange(expiredFeeds);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted {Count} expired feeds older than {CutoffDate}", expiredFeeds.Count, cutoffDate);
        }
    }

    public async Task<(int totalFeeds, int validFeeds, int expiredFeeds)> GetFeedStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var totalFeeds = await _context.PersonalizedFeeds.CountAsync();
        var validFeeds = await _context.PersonalizedFeeds.CountAsync(pf => pf.ExpiresAt > now);
        var expiredFeeds = totalFeeds - validFeeds;

        return (totalFeeds, validFeeds, expiredFeeds);
    }
}
