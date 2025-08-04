namespace AdminService.Models.DTOs
{
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int DailySignups { get; set; }
        public int WeeklySignups { get; set; }
        public int MonthlySignups { get; set; }
        public double GrowthRate { get; set; }
        public int BannedUsers { get; set; }
        public int DailyBans { get; set; }
        public int WeeklyBans { get; set; }
        public int MonthlyBans { get; set; }
    }
}
