namespace AdminService.Models.DTOs
{
    public class BlogPostModerationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int CommentsCount { get; set; }
        public double AverageRating { get; set; }
        public int ReportsCount { get; set; }
        public int FlagsCount { get; set; }
        public bool IsFeatured { get; set; }
        public List<ContentFlagDto> Flags { get; set; } = new List<ContentFlagDto>();
    }

    public class CommentModerationDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public Guid BlogPostId { get; set; }
        public string BlogPostTitle { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int ReportsCount { get; set; }
        public int FlagsCount { get; set; }
        public List<ContentFlagDto> Flags { get; set; } = new List<ContentFlagDto>();
    }

    public class ContentFlagDto
    {
        public Guid Id { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string ContentId { get; set; } = string.Empty;
        public string FlagType { get; set; } = string.Empty;
        public string? FlaggedBy { get; set; }
        public bool AutoDetected { get; set; }
        public int Severity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserReportDto
    {
        public Guid Id { get; set; }
        public string ReportedUserId { get; set; } = string.Empty;
        public string ReportedUserName { get; set; } = string.Empty;
        public string? ReporterUserId { get; set; }
        public string? ReporterUserName { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}