namespace AdminService.Models.DTOs
{
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int BannedUsers { get; set; }
        public double AverageSessionDuration { get; set; }
        public List<UserRegistrationTrendDto> RegistrationTrend { get; set; } = new();
        public List<UserActivityDto> UserActivity { get; set; } = new();
    }

    public class UserRegistrationTrendDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class UserActivityDto
    {
        public DateTime Date { get; set; }
        public int ActiveUsers { get; set; }
        public int LoginCount { get; set; }
    }
}