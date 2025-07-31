namespace AdminService.Models.DTOs
{
    public class ContentStatsDto
    {
        public int TotalBlogPosts { get; set; }
        public int PublishedBlogPosts { get; set; }
        public int DraftBlogPosts { get; set; }
        public int BlogPostsToday { get; set; }
        public int TotalComments { get; set; }
        public int CommentsToday { get; set; }
        public int BlogPostsThisWeek { get; set; }
        public int CommentsThisWeek { get; set; }
        public int BlogPostsThisMonth { get; set; }
        public int CommentsThisMonth { get; set; }
        public double AverageRating { get; set; }
        public List<DailyMetricDto> DailyContent { get; set; } = new List<DailyMetricDto>();
        public List<TopContentDto> TopPosts { get; set; } = new List<TopContentDto>();
        public List<ContentTrendDto> BlogPostTrend { get; set; } = new();
        public List<ContentTrendDto> CommentTrend { get; set; } = new();
        public List<PopularContentDto> PopularBlogPosts { get; set; } = new();
    }

    public class ContentTrendDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class PopularContentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}