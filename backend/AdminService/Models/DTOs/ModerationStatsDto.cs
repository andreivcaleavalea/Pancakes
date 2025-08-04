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
    }
}
