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
    }
}
