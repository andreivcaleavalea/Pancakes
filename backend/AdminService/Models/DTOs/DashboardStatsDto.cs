namespace AdminService.Models.DTOs
{
    public class DashboardStatsDto
    {
        public UserStatsDto UserStats { get; set; } = new UserStatsDto();
        public ContentStatsDto ContentStats { get; set; } = new ContentStatsDto();
        public ModerationStatsDto ModerationStats { get; set; } = new ModerationStatsDto();
        public SystemStatsDto SystemStats { get; set; } = new SystemStatsDto();
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