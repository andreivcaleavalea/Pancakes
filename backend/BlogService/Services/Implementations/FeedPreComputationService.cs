using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

/// <summary>
/// Background service that pre-computes personalized feeds for all users
/// Runs every 15-30 minutes to keep feeds fresh
/// </summary>
public class FeedPreComputationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FeedPreComputationService> _logger;
    private readonly TimeSpan _computationInterval = TimeSpan.FromMinutes(20); // Run every 20 minutes

    public FeedPreComputationService(IServiceProvider serviceProvider, ILogger<FeedPreComputationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Feed Pre-Computation Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PreComputeAllUserFeedsAsync();
                await Task.Delay(_computationInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Feed Pre-Computation Service stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Feed Pre-Computation Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
            }
        }

        _logger.LogInformation("Feed Pre-Computation Service stopped");
    }

    private async Task PreComputeAllUserFeedsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var personalizedFeedRepository = scope.ServiceProvider.GetRequiredService<IPersonalizedFeedRepository>();
        var recommendationService = scope.ServiceProvider.GetRequiredService<RecommendationService>();
        var userInterestService = scope.ServiceProvider.GetRequiredService<IUserInterestService>();

        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Starting feed pre-computation cycle at {StartTime}", startTime);

            // 1. Get users that need feed updates
            var expiredFeeds = await personalizedFeedRepository.GetExpiredFeedsAsync();
            var expiringFeeds = await personalizedFeedRepository.GetExpiringFeedsAsync(5); // Expiring in 5 minutes
            var usersWithoutFeeds = await personalizedFeedRepository.GetUsersWithoutFeedsAsync();

            var usersToUpdate = expiredFeeds.Select(f => f.UserId)
                .Union(expiringFeeds.Select(f => f.UserId))
                .Union(usersWithoutFeeds)
                .Distinct()
                .ToList();

            _logger.LogInformation("Found {UserCount} users needing feed updates: {ExpiredCount} expired, {ExpiringCount} expiring, {NewCount} new", 
                usersToUpdate.Count, expiredFeeds.Count(), expiringFeeds.Count(), usersWithoutFeeds.Count());

            // 2. Pre-compute feeds for each user (with parallelization for performance)
            var computationTasks = usersToUpdate.Select(async userId =>
            {
                try
                {
                    await recommendationService.PreComputeUserFeedAsync(userId);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to pre-compute feed for user {UserId}", userId);
                    return false;
                }
            });

            var results = await Task.WhenAll(computationTasks);
            var successCount = results.Count(r => r);
            var failureCount = results.Count(r => !r);

            // 3. Perform maintenance tasks
            await PerformMaintenanceTasksAsync(userInterestService, personalizedFeedRepository);

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogInformation("Feed pre-computation cycle completed in {Duration}. Success: {SuccessCount}, Failures: {FailureCount}", 
                duration, successCount, failureCount);

            // 4. Log statistics
            await LogFeedStatisticsAsync(personalizedFeedRepository);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during feed pre-computation cycle");
        }
    }

    private async Task PerformMaintenanceTasksAsync(IUserInterestService userInterestService, IPersonalizedFeedRepository personalizedFeedRepository)
    {
        try
        {
            // Decay user interests (daily)
            var lastDecay = DateTime.UtcNow.Date;
            await userInterestService.DecayInterestsAsync();

            // Clean up low-score interests (weekly)
            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday)
            {
                await userInterestService.CleanupLowScoreInterestsAsync();
            }

            // Delete very old expired feeds (weekly)
            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
            {
                await personalizedFeedRepository.DeleteExpiredFeedsAsync(7);
            }

            _logger.LogDebug("Completed maintenance tasks");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during maintenance tasks");
        }
    }

    private async Task LogFeedStatisticsAsync(IPersonalizedFeedRepository personalizedFeedRepository)
    {
        try
        {
            var (totalFeeds, validFeeds, expiredFeeds) = await personalizedFeedRepository.GetFeedStatisticsAsync();
            
            _logger.LogInformation("Feed Statistics: Total={TotalFeeds}, Valid={ValidFeeds}, Expired={ExpiredFeeds}, ValidPercentage={ValidPercentage:F1}%", 
                totalFeeds, validFeeds, expiredFeeds, totalFeeds > 0 ? (validFeeds * 100.0 / totalFeeds) : 0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error logging feed statistics");
        }
    }
}

/// <summary>
/// Service interface for accessing the enhanced recommendation service in the background service
/// </summary>
public interface IFeedPreComputationService
{
    Task PreComputeUserFeedAsync(string userId);
}

/// <summary>
/// Wrapper service to access RecommendationService methods
/// </summary>
public class FeedPreComputationServiceWrapper : IFeedPreComputationService
{
    private readonly RecommendationService _recommendationService;

    public FeedPreComputationServiceWrapper(RecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    public async Task PreComputeUserFeedAsync(string userId)
    {
        await _recommendationService.PreComputeUserFeedAsync(userId);
    }
}
