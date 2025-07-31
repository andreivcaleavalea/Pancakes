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
        public List<ServiceStatusDto> ServiceStatuses { get; set; } = new List<ServiceStatusDto>();
        public List<ServiceHealthDto> ServiceHealth { get; set; } = new();
        public List<SystemResourceTrendDto> ResourceTrend { get; set; } = new();
    }

    public class ServiceHealthDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public long ResponseTimeMs { get; set; }
    }

    public class SystemResourceTrendDto
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
    }
}