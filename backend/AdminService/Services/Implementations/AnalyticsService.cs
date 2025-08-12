using AdminService.Clients.BlogClient.Services;
using AdminService.Clients.UserClient;
using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdminService.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AdminDbContext _context;
        private readonly UserServiceClient _userServiceClient;
        private readonly BlogServiceClient _blogServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditService _auditService;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            AdminDbContext context,
            UserServiceClient userServiceClient,
            BlogServiceClient blogServiceClient,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService,
            ILogger<AnalyticsService> logger)
        {
            _context = context;
            _userServiceClient = userServiceClient;
            _blogServiceClient = blogServiceClient;
            _httpContextAccessor = httpContextAccessor;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                // Log dashboard access
                await LogDashboardAccessAsync();

                // Get current admin user's last dashboard access time
                var lastAccessTime = await GetLastDashboardAccessTimeAsync();
                _logger.LogInformation("Calculating stats since last access: {LastAccess}", lastAccessTime);

                // Get user statistics
                var userStats = await GetUserStatsAsync(lastAccessTime);
                
                // Get content statistics  
                var contentStats = await GetContentStatsAsync(lastAccessTime);

                return new DashboardStatsDto
                {
                    UserStats = userStats,
                    ContentStats = contentStats,
                    ModerationStats = new ModerationStatsDto(),
                    SystemStats = new SystemStatsDto
                    {
                        DatabaseStatus = "Healthy",
                        ServiceVersion = "1.0.0",
                        LastMetricCollection = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                // Return empty stats on error
                return new DashboardStatsDto
                {
                    UserStats = new UserStatsDto(),
                    ContentStats = new ContentStatsDto(),
                    ModerationStats = new ModerationStatsDto(),
                    SystemStats = new SystemStatsDto
                    {
                        DatabaseStatus = "Error",
                        ServiceVersion = "1.0.0",
                        LastMetricCollection = DateTime.UtcNow
                    }
                };
            }
        }

        private async Task LogDashboardAccessAsync()
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("id")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    await _auditService.LogActionAsync(userIdClaim, "DASHBOARD_ACCESS", "Dashboard", userIdClaim, 
                        new { AccessTime = DateTime.UtcNow }, "", "");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging dashboard access");
            }
        }

        private async Task<DateTime> GetLastDashboardAccessTimeAsync()
        {
            try
            {
                // Get current admin user ID from claims
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Could not get admin ID from claims");
                    return DateTime.UtcNow.AddDays(-1); // Default to 1 day ago
                }

                // Get the last dashboard access from audit logs (excluding the current one we just logged)
                var lastAccess = await _context.AdminAuditLogs
                    .Where(log => log.AdminId == userIdClaim && 
                                  log.Action == "DASHBOARD_ACCESS" && 
                                  log.Timestamp < DateTime.UtcNow.AddMinutes(-1)) // Exclude very recent logs
                    .OrderByDescending(log => log.Timestamp)
                    .FirstOrDefaultAsync();

                if (lastAccess != null)
                {
                    return lastAccess.Timestamp;
                }

                // If no previous access found, use last login time
                var admin = await _context.AdminUsers.FindAsync(userIdClaim);
                return admin?.LastLoginAt.AddDays(-1) ?? DateTime.UtcNow.AddDays(-1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last dashboard access time");
                return DateTime.UtcNow.AddDays(-1);
            }
        }

        private async Task<UserStatsDto> GetUserStatsAsync(DateTime sinceTime)
        {
            try
            {
                // Get total users count by searching with a large page size
                var allUsersResponse = await _userServiceClient.SearchUsersAsync(new UserSearchRequest
                {
                    Page = 1,
                    PageSize = 1, // Just get the total count
                    SearchTerm = ""
                });

                var totalUsers = allUsersResponse.TotalCount;

                // For new users since last login, we'll make another call
                // Note: This is a simplified approach. Ideally, the UserService would have a "created since" filter
                var newUsersCount = 0; // TODO: Implement proper date filtering in UserService

                return new UserStatsDto
                {
                    TotalUsers = totalUsers,
                    DailySignups = newUsersCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user stats");
                return new UserStatsDto();
            }
        }

        private async Task<ContentStatsDto> GetContentStatsAsync(DateTime sinceTime)
        {
            try
            {
                // Get total blog posts count
                var allPostsResponse = await _blogServiceClient.SearchBlogPostsAsync(new BlogPostSearchRequest
                {
                    Page = 1,
                    PageSize = 1, // Just get the total count
                    Search = ""
                });

                var totalPosts = allPostsResponse.TotalCount;

                // For new posts since last login, we'll make another call with date filter
                var newPostsResponse = await _blogServiceClient.SearchBlogPostsAsync(new BlogPostSearchRequest
                {
                    Page = 1,
                    PageSize = int.MaxValue, // Get all to count
                    DateFrom = sinceTime,
                    Search = ""
                });

                var newPostsCount = newPostsResponse.TotalCount;

                return new ContentStatsDto
                {
                    TotalBlogPosts = totalPosts,
                    BlogPostsToday = newPostsCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content stats");
                return new ContentStatsDto();
            }
        }

        public Task<object> GetDetailedAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return Task.FromResult<object>(new
            {
                FromDate = fromDate,
                ToDate = toDate,
                Message = "Detailed analytics coming soon"
            });
        }
    }
}
