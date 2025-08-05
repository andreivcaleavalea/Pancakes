using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Requests
{
    public class BlogPostSearchRequest
    {
        public string? Search { get; set; }
        public string? AuthorId { get; set; }
        public int? Status { get; set; } // 0 = Draft, 1 = Published, 2 = Archived
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }

    public class UpdateBlogPostStatusRequest
    {
        [Required]
        public string BlogPostId { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 2, ErrorMessage = "Status must be 0 (Draft), 1 (Published), or 2 (Archived)")]
        public int Status { get; set; }
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
    }

    public class DeleteBlogPostRequest
    {
        [Required]
        public string BlogPostId { get; set; } = string.Empty;
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
        
        public bool DeleteComments { get; set; } = true;
    }
}
