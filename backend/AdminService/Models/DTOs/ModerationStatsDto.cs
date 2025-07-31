namespace AdminService.Models.DTOs
{
    public class ModerationStatsDto
    {
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int TotalFlags { get; set; }
        public int PendingFlags { get; set; }
        public int BannedUsers { get; set; }
        public int DeletedPosts { get; set; }
        public int DeletedComments { get; set; }
        public int ResolvedFlags { get; set; }
        public int ResolvedReports { get; set; }
        public int FlagsToday { get; set; }
        public int ReportsToday { get; set; }
        public List<ModerationActivityDto> RecentActivity { get; set; } = new List<ModerationActivityDto>();
        public List<FlagTypeStatsDto> FlagsByType { get; set; } = new();
        public List<ModerationTrendDto> ModerationTrend { get; set; } = new();
    }

    public class FlagTypeStatsDto
    {
        public string FlagType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int PendingCount { get; set; }
    }

    public class ModerationTrendDto
    {
        public DateTime Date { get; set; }
        public int FlagCount { get; set; }
        public int ReportCount { get; set; }
    }
}