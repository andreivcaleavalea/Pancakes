namespace AdminService.Models.DTOs
{
    public class SystemMetricDto
    {
        public string Id { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double? Threshold { get; set; }
        public bool IsAlert { get; set; }
    }
}
