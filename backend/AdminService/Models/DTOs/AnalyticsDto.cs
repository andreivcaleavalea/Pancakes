namespace AdminService.Models.DTOs
{
    public class DashboardStatsDto
    {
        public UserStatsDto UserStats { get; set; } = new UserStatsDto();
        public ContentStatsDto ContentStats { get; set; } = new ContentStatsDto();
        public ModerationStatsDto ModerationStats { get; set; } = new ModerationStatsDto();
        public SystemStatsDto SystemStats { get; set; } = new SystemStatsDto();
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int DailySignups { get; set; }
        public int WeeklySignups { get; set; }
        public int MonthlySignups { get; set; }
        public double GrowthRate { get; set; }
        public List<DailyMetricDto> DailyGrowth { get; set; } = new List<DailyMetricDto>();
    }

    public class ContentStatsDto
    {
        public int TotalBlogPosts { get; set; }
        public int PublishedBlogPosts { get; set; }
        public int DraftBlogPosts { get; set; }
        public int BlogPostsToday { get; set; }
        public int TotalComments { get; set; }
        public int CommentsToday { get; set; }
        public double AverageRating { get; set; }
        public List<DailyMetricDto> DailyContent { get; set; } = new List<DailyMetricDto>();
        public List<TopContentDto> TopPosts { get; set; } = new List<TopContentDto>();
    }

    public class ModerationStatsDto
    {
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int TotalFlags { get; set; }
        public int PendingFlags { get; set; }
        public int BannedUsers { get; set; }
        public int DeletedPosts { get; set; }
        public int DeletedComments { get; set; }
        public List<ModerationActivityDto> RecentActivity { get; set; } = new List<ModerationActivityDto>();
    }

    public class SystemStatsDto
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorsLastHour { get; set; }
        public List<ServiceStatusDto> ServiceStatuses { get; set; } = new List<ServiceStatusDto>();
    }

    public class DailyMetricDto
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public string MetricType { get; set; } = string.Empty;
    }

    public class TopContentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int Views { get; set; }
        public double Rating { get; set; }
        public int CommentsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ModerationActivityDto
    {
        public string AdminName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ServiceStatusDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // healthy, degraded, down
        public double ResponseTime { get; set; }
        public DateTime LastCheck { get; set; }
    }

    public class SystemMetricDto
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int DailySignups { get; set; }
        public int OnlineUsers { get; set; }
        public int TotalBlogPosts { get; set; }
        public int BlogPostsCreatedToday { get; set; }
        public int TotalComments { get; set; }
        public int CommentsPostedToday { get; set; }
        public double AverageSessionDuration { get; set; }
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int TotalFlags { get; set; }
        public int PendingFlags { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
    }
}