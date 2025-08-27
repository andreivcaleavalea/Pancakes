namespace UserService.Models.Requests;

public class PortfolioQueryParameters : PaginationParameters
{
    public string? Search { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "desc";
}
