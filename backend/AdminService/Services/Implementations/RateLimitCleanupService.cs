using AdminService.Middleware;

namespace AdminService.Services.Implementations
{
    public class RateLimitCleanupService : BackgroundService
    {
        private readonly ILogger<RateLimitCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(30); // Cleanup every 30 minutes

        public RateLimitCleanupService(ILogger<RateLimitCleanupService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rate limit cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Cleaning up expired rate limit clients");
                    RateLimitingMiddleware.CleanupExpiredClients();
                    _logger.LogDebug("Rate limit cleanup completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during rate limit cleanup");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Rate limit cleanup service stopped");
        }
    }
}