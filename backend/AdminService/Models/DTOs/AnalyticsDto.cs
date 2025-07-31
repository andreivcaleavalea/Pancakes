namespace AdminService.Models.DTOs
{
    // Note: Main DTO classes are defined in their individual files
    // This file contains supporting DTOs used by the main analytics DTOs

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