using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class UserInterestRepository : IUserInterestRepository
{
    private readonly BlogDbContext _context;
    private readonly ILogger<UserInterestRepository> _logger;

    public UserInterestRepository(BlogDbContext context, ILogger<UserInterestRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UserInterest>> GetUserInterestsAsync(string userId)
    {
        return await _context.UserInterests
            .Where(ui => ui.UserId == userId)
            .OrderByDescending(ui => ui.Score)
            .ToListAsync();
    }

    public async Task<UserInterest?> GetUserInterestAsync(string userId, string tag)
    {
        return await _context.UserInterests
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.Tag == tag);
    }

    public async Task<UserInterest> UpsertUserInterestAsync(string userId, string tag, double scoreIncrement)
    {
        var existingInterest = await GetUserInterestAsync(userId, tag);
        
        if (existingInterest != null)
        {
            // Apply time decay before adding new score
            var timeSinceUpdate = DateTime.UtcNow - existingInterest.LastUpdated;
            var daysSinceUpdate = timeSinceUpdate.TotalDays;
            var decayFactor = Math.Pow(0.98, daysSinceUpdate); // 2% daily decay
            
            existingInterest.Score = (existingInterest.Score * decayFactor) + scoreIncrement;
            existingInterest.InteractionCount++;
            existingInterest.LastUpdated = DateTime.UtcNow;
            
            return existingInterest;
        }
        else
        {
            var newInterest = new UserInterest
            {
                UserId = userId,
                Tag = tag,
                Score = scoreIncrement,
                InteractionCount = 1,
                LastUpdated = DateTime.UtcNow
            };
            
            _context.UserInterests.Add(newInterest);
            await _context.SaveChangesAsync();
            return newInterest;
        }
    }

    public async Task UpdateUserInterestsAsync(string userId, Dictionary<string, double> tagScoreIncrements)
    {
        var existingInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userId && tagScoreIncrements.Keys.Contains(ui.Tag))
            .ToListAsync();

        var existingTags = existingInterests.ToDictionary(ui => ui.Tag, ui => ui);
        var newInterests = new List<UserInterest>();

        foreach (var (tag, scoreIncrement) in tagScoreIncrements)
        {
            if (existingTags.TryGetValue(tag, out var existingInterest))
            {
                // Apply time decay before adding new score
                var timeSinceUpdate = DateTime.UtcNow - existingInterest.LastUpdated;
                var daysSinceUpdate = timeSinceUpdate.TotalDays;
                var decayFactor = Math.Pow(0.98, daysSinceUpdate); // 2% daily decay
                
                existingInterest.Score = (existingInterest.Score * decayFactor) + scoreIncrement;
                existingInterest.InteractionCount++;
                existingInterest.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                newInterests.Add(new UserInterest
                {
                    UserId = userId,
                    Tag = tag,
                    Score = scoreIncrement,
                    InteractionCount = 1,
                    LastUpdated = DateTime.UtcNow
                });
            }
        }

        if (newInterests.Any())
        {
            _context.UserInterests.AddRange(newInterests);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DecayAllInterestsAsync(double decayFactor = 0.95)
    {
        // Use raw SQL for efficiency on large datasets
        var sql = @"
            UPDATE ""UserInterests"" 
            SET ""Score"" = ""Score"" * {0},
                ""LastUpdated"" = {1}
            WHERE ""LastUpdated"" < {2}";

        var cutoffDate = DateTime.UtcNow.AddDays(-1); // Only decay interests older than 1 day
        var now = DateTime.UtcNow;

        await _context.Database.ExecuteSqlRawAsync(sql, decayFactor, now, cutoffDate);
        _logger.LogInformation("Applied decay factor {DecayFactor} to interests older than {CutoffDate}", decayFactor, cutoffDate);
    }

    public async Task<Dictionary<string, double>> GetTopUserInterestsAsync(string userId, int topCount = 20)
    {
        var interests = await _context.UserInterests
            .Where(ui => ui.UserId == userId)
            .OrderByDescending(ui => ui.Score)
            .Take(topCount)
            .ToDictionaryAsync(ui => ui.Tag, ui => ui.Score);

        // Apply time decay on the fly for the most accurate scores
        var now = DateTime.UtcNow;
        var decayedInterests = new Dictionary<string, double>();
        
        foreach (var (tag, score) in interests)
        {
            var interest = await _context.UserInterests
                .FirstAsync(ui => ui.UserId == userId && ui.Tag == tag);
            
            var timeSinceUpdate = now - interest.LastUpdated;
            var daysSinceUpdate = timeSinceUpdate.TotalDays;
            var decayFactor = Math.Pow(0.98, daysSinceUpdate);
            
            decayedInterests[tag] = score * decayFactor;
        }

        return decayedInterests.Where(kvp => kvp.Value > 0.01)
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task CleanupLowScoreInterestsAsync(double minimumScore = 0.01)
    {
        var sql = @"DELETE FROM ""UserInterests"" WHERE ""Score"" < {0}";
        var deletedCount = await _context.Database.ExecuteSqlRawAsync(sql, minimumScore);
        _logger.LogInformation("Cleaned up {DeletedCount} low-score interests below {MinimumScore}", deletedCount, minimumScore);
    }
}
