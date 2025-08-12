namespace BlogService.Models.DTOs;

public class ReportStatsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ResolvedReports { get; set; }
    public int DismissedReports { get; set; }
}

