namespace AdminService.Models.DTOs
{
    public class DashboardStatsDto
    {
        public UserStatsDto UserStats { get; set; } = new UserStatsDto();
        public ContentStatsDto ContentStats { get; set; } = new ContentStatsDto();
        public ModerationStatsDto ModerationStats { get; set; } = new ModerationStatsDto();
        public SystemStatsDto SystemStats { get; set; } = new SystemStatsDto();
    }
}
