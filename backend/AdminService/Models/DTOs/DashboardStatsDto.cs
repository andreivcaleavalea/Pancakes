namespace AdminService.Models.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalBlogPosts { get; set; }
        public int TotalComments { get; set; }
        public int PendingFlags { get; set; }
        public int PendingReports { get; set; }
        public int DailySignups { get; set; }
        public int BlogPostsCreatedToday { get; set; }
        public int CommentsPostedToday { get; set; }
        public SystemResourcesDto SystemResources { get; set; } = new();
        public List<DailyMetricDto> RecentMetrics { get; set; } = new();
    }

    public class SystemResourcesDto
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
    }

    public class DailyMetricDto
    {
        public DateTime Date { get; set; }
        public string MetricType { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}