using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Requests
{
    public class ContentSearchRequest
    {
        public string? SearchTerm { get; set; }
        public string? ContentType { get; set; } // "blog_post", "comment"
        public string? AuthorId { get; set; }
        public string? Status { get; set; }
        public bool? HasFlags { get; set; }
        public bool? HasReports { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }

    public class ModerateBlogPostRequest
    {
        [Required]
        public Guid BlogPostId { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty; // approve, reject, delete, feature, unfeature
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
        
        public bool NotifyAuthor { get; set; } = true;
    }

    public class ModerateCommentRequest
    {
        [Required]
        public Guid CommentId { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty; // approve, delete, shadow_ban
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
        
        public bool NotifyAuthor { get; set; } = true;
    }

    public class BulkModerationRequest
    {
        [Required]
        public List<string> ContentIds { get; set; } = new List<string>();
        
        [Required]
        public string ContentType { get; set; } = string.Empty; // "blog_post", "comment"
        
        [Required]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
        
        public bool NotifyAuthors { get; set; } = true;
    }

    public class CreateContentFlagRequest
    {
        [Required]
        public string ContentType { get; set; } = string.Empty;
        
        [Required]
        public string ContentId { get; set; } = string.Empty;
        
        [Required]
        public string FlagType { get; set; } = string.Empty;
        
        public string? FlaggedBy { get; set; }
        
        public bool AutoDetected { get; set; } = false;
        
        [Range(1, 5)]
        public int Severity { get; set; } = 1;
        
        public string Description { get; set; } = string.Empty;
    }

    public class ReviewFlagRequest
    {
        [Required]
        public Guid FlagId { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty; // dismiss, action_taken
        
        [Required]
        [MinLength(10)]
        public string ReviewNotes { get; set; } = string.Empty;
    }

    public class CreateUserReportRequest
    {
        [Required]
        public string ReportedUserId { get; set; } = string.Empty;
        
        public string? ReporterUserId { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
    }

    public class ReviewUserReportRequest
    {
        [Required]
        public Guid ReportId { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty; // dismiss, action_taken
        
        [Required]
        [MinLength(10)]
        public string ReviewNotes { get; set; } = string.Empty;
    }
}