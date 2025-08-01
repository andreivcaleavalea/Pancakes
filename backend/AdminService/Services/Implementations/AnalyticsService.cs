using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Models.Entities;
using AdminService.Services.Interfaces;
using AdminService.Clients;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AdminDbContext _context;
        private readonly UserServiceClient _userServiceClient;
        private readonly BlogServiceClient _blogServiceClient;
        private readonly IMapper _mapper;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            AdminDbContext context,
            UserServiceClient userServiceClient,
            BlogServiceClient blogServiceClient,
            IMapper mapper,
            ILogger<AnalyticsService> logger)
        {
            _context = context;
            _userServiceClient = userServiceClient;
            _blogServiceClient = blogServiceClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var latestMetrics = await GetLatestSystemMetricsAsync();
            
            // Calculate growth rate based on weekly data
            var previousWeekMetrics = await GetMetricsFromDaysAgoAsync(7);
            var growthRate = CalculateGrowthRate(latestMetrics?.TotalUsers ?? 0, previousWeekMetrics?.TotalUsers ?? 0);
            
            return new DashboardStatsDto
            {
                UserStats = new UserStatsDto
                {
                    TotalUsers = latestMetrics?.TotalUsers ?? 0,
                    ActiveUsers = latestMetrics?.ActiveUsers ?? 0,
                    OnlineUsers = latestMetrics?.OnlineUsers ?? 0,
                    DailySignups = latestMetrics?.DailySignups ?? 0,
                    WeeklySignups = await GetSignupsInLastDaysAsync(7),
                    MonthlySignups = await GetSignupsInLastDaysAsync(30),
                    GrowthRate = growthRate,
                    BannedUsers = await GetBannedUsersCountAsync()
                },
                ContentStats = new ContentStatsDto
                {
                    TotalBlogPosts = latestMetrics?.TotalBlogPosts ?? 0,
                    BlogPostsToday = latestMetrics?.BlogPostsCreatedToday ?? 0,
                    TotalComments = latestMetrics?.TotalComments ?? 0,
                    CommentsToday = latestMetrics?.CommentsPostedToday ?? 0,
                    PublishedBlogPosts = latestMetrics?.TotalBlogPosts ?? 0, // Assuming all are published for now
                    DraftBlogPosts = 0, // TODO: Get actual draft count from BlogService
                    AverageRating = 0.0 // TODO: Calculate actual average rating
                },
                ModerationStats = new ModerationStatsDto
                {
                    TotalReports = latestMetrics?.TotalReports ?? 0,
                    PendingReports = latestMetrics?.PendingReports ?? 0,
                    TotalFlags = latestMetrics?.TotalFlags ?? 0,
                    PendingFlags = latestMetrics?.PendingFlags ?? 0,
                    BannedUsers = await GetBannedUsersCountAsync(),
                    DeletedPosts = 0, // TODO: Get actual deleted posts count
                    DeletedComments = 0 // TODO: Get actual deleted comments count
                },
                SystemStats = new SystemStatsDto
                {
                    CpuUsage = latestMetrics?.CpuUsage ?? 0,
                    MemoryUsage = latestMetrics?.MemoryUsage ?? 0,
                    DiskUsage = latestMetrics?.DiskUsage ?? 0,
                    AverageResponseTime = 0.0, // TODO: Calculate actual response time
                    ErrorsLastHour = 0, // TODO: Get actual error count
                    DatabaseStatus = "Connected", // TODO: Check actual database status
                    ServiceVersion = "1.0.0",
                    LastMetricCollection = latestMetrics?.Timestamp ?? DateTime.UtcNow
                }
            };
        }

        public async Task<UserStatsDto> GetUserStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var stats = await _userServiceClient.GetUserStatisticsAsync();
            
            return new UserStatsDto
            {
                TotalUsers = stats.ContainsKey("totalUsers") ? Convert.ToInt32(stats["totalUsers"]) : 0,
                ActiveUsers = stats.ContainsKey("activeUsers") ? Convert.ToInt32(stats["activeUsers"]) : 0
            };
        }

        public async Task<ContentStatsDto> GetContentStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var stats = await _blogServiceClient.GetContentStatisticsAsync();
            
            return new ContentStatsDto
            {
                TotalBlogPosts = stats.ContainsKey("totalPosts") ? Convert.ToInt32(stats["totalPosts"]) : 0,
                TotalComments = stats.ContainsKey("totalComments") ? Convert.ToInt32(stats["totalComments"]) : 0
            };
        }

        public async Task<ModerationStatsDto> GetModerationStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var totalReports = await _context.UserReports.CountAsync();
            var pendingReports = await _context.UserReports.CountAsync(r => r.Status == "pending");
            var totalFlags = await _context.ContentFlags.CountAsync();
            var pendingFlags = await _context.ContentFlags.CountAsync(f => f.Status == "pending");

            return new ModerationStatsDto
            {
                TotalReports = totalReports,
                PendingReports = pendingReports,
                TotalFlags = totalFlags,
                PendingFlags = pendingFlags
            };
        }

        public async Task<SystemStatsDto> GetSystemStatsAsync()
        {
            var latestMetrics = await GetLatestSystemMetricsAsync();
            
            return new SystemStatsDto
            {
                CpuUsage = latestMetrics?.CpuUsage ?? 0,
                MemoryUsage = latestMetrics?.MemoryUsage ?? 0,
                DiskUsage = latestMetrics?.DiskUsage ?? 0
            };
        }

        public async Task<List<DailyMetricDto>> GetDailyMetricsAsync(string metricType, DateTime fromDate, DateTime toDate)
        {
            var metrics = await _context.SystemMetrics
                .Where(m => m.Timestamp >= fromDate && m.Timestamp <= toDate)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return metrics.Select(m => new DailyMetricDto
            {
                Date = m.Timestamp.Date,
                Value = metricType switch
                {
                    "users" => m.DailySignups,
                    "posts" => m.BlogPostsCreatedToday,
                    "comments" => m.CommentsPostedToday,
                    _ => 0
                },
                MetricType = metricType
            }).ToList();
        }

        public async Task<List<TopContentDto>> GetTopContentAsync(int count = 10)
        {
            // Would integrate with BlogService to get actual top content
            return new List<TopContentDto>();
        }

        public async Task<List<ModerationActivityDto>> GetRecentModerationActivityAsync(int count = 20)
        {
            var auditLogs = await _context.AdminAuditLogs
                .Where(a => a.Action.Contains("MODERATE") || a.Action.Contains("BAN") || a.Action.Contains("DELETE"))
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();

            return auditLogs.Select(a => new ModerationActivityDto
            {
                AdminName = a.AdminId, // Would resolve to actual admin name
                Action = a.Action,
                TargetType = a.TargetType,
                TargetId = a.TargetId,
                Timestamp = a.Timestamp
            }).ToList();
        }

        public async Task<List<ServiceStatusDto>> GetServiceStatusesAsync()
        {
            return new List<ServiceStatusDto>
            {
                new ServiceStatusDto { ServiceName = "UserService", Status = "healthy", ResponseTime = 120, LastCheck = DateTime.UtcNow },
                new ServiceStatusDto { ServiceName = "BlogService", Status = "healthy", ResponseTime = 150, LastCheck = DateTime.UtcNow },
                new ServiceStatusDto { ServiceName = "AdminService", Status = "healthy", ResponseTime = 80, LastCheck = DateTime.UtcNow }
            };
        }

        public async Task CollectSystemMetricsAsync()
        {
            try
            {
                var userStats = await _userServiceClient.GetUserStatisticsAsync();
                var contentStats = await _blogServiceClient.GetContentStatisticsAsync();
                var moderationStats = await GetModerationStatsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

                var metrics = new SystemMetric
                {
                    TotalUsers = userStats.ContainsKey("totalUsers") ? Convert.ToInt32(userStats["totalUsers"]) : 0,
                    ActiveUsers = userStats.ContainsKey("activeUsers") ? Convert.ToInt32(userStats["activeUsers"]) : 0,
                    DailySignups = userStats.ContainsKey("dailySignups") ? Convert.ToInt32(userStats["dailySignups"]) : 0,
                    TotalBlogPosts = contentStats.ContainsKey("totalPosts") ? Convert.ToInt32(contentStats["totalPosts"]) : 0,
                    TotalComments = contentStats.ContainsKey("totalComments") ? Convert.ToInt32(contentStats["totalComments"]) : 0,
                    TotalReports = moderationStats.TotalReports,
                    PendingReports = moderationStats.PendingReports,
                    TotalFlags = moderationStats.TotalFlags,
                    PendingFlags = moderationStats.PendingFlags,
                    CpuUsage = GetRandomMetric(10, 80),
                    MemoryUsage = GetRandomMetric(30, 70),
                    DiskUsage = GetRandomMetric(20, 60)
                };

                _context.SystemMetrics.Add(metrics);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting system metrics");
            }
        }

        public async Task<SystemMetricDto> GetLatestSystemMetricsAsync()
        {
            var latestMetric = await _context.SystemMetrics
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();

            return latestMetric != null ? _mapper.Map<SystemMetricDto>(latestMetric) : new SystemMetricDto();
        }

        public async Task<List<SystemMetricDto>> GetHistoricalMetricsAsync(DateTime fromDate, DateTime toDate)
        {
            var metrics = await _context.SystemMetrics
                .Where(m => m.Timestamp >= fromDate && m.Timestamp <= toDate)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return _mapper.Map<List<SystemMetricDto>>(metrics);
        }

        private static double GetRandomMetric(double min, double max)
        {
            var random = new Random();
            return Math.Round(random.NextDouble() * (max - min) + min, 2);
        }

        private async Task<SystemMetricDto?> GetMetricsFromDaysAgoAsync(int daysAgo)
        {
            var targetDate = DateTime.UtcNow.AddDays(-daysAgo);
            var metric = await _context.SystemMetrics
                .Where(m => m.Timestamp.Date == targetDate.Date)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();

            return metric != null ? _mapper.Map<SystemMetricDto>(metric) : null;
        }

        private double CalculateGrowthRate(int currentValue, int previousValue)
        {
            if (previousValue == 0) return currentValue > 0 ? 100.0 : 0.0;
            return Math.Round(((double)(currentValue - previousValue) / previousValue) * 100, 2);
        }

        private async Task<int> GetSignupsInLastDaysAsync(int days)
        {
            try
            {
                var stats = await _userServiceClient.GetUserStatisticsAsync();
                
                // For now, return a simple calculation based on daily signups
                // TODO: Implement proper date-range queries in UserService
                var dailySignups = stats.ContainsKey("dailySignups") ? Convert.ToInt32(stats["dailySignups"]) : 0;
                return dailySignups * days; // Simple approximation
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get signup statistics for last {Days} days", days);
                return 0;
            }
        }

        private async Task<int> GetBannedUsersCountAsync()
        {
            try
            {
                var stats = await _userServiceClient.GetUserStatisticsAsync();
                return stats.ContainsKey("bannedUsers") ? Convert.ToInt32(stats["bannedUsers"]) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get banned users count");
                return 0;
            }
        }
    }
}