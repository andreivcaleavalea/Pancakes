using BlogService.Models.Entities;

namespace BlogService.Models.Requests;

public class BlogPostQueryParameters : PaginationParameters
{
    public string? Search { get; set; }
    public Guid? AuthorId { get; set; }
    public PostStatus? Status { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "desc";
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
