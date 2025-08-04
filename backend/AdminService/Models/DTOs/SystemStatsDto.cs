namespace AdminService.Models.DTOs
{
    public class SystemStatsDto
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorsLastHour { get; set; }
        public int OnlineUsers { get; set; }
        public string DatabaseStatus { get; set; } = string.Empty;
        public string ServiceVersion { get; set; } = string.Empty;
        public DateTime LastMetricCollection { get; set; }
    }
}
