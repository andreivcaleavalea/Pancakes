using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Entities
{
    public class SystemMetric
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // User metrics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; } // Last 30 days
        public int DailySignups { get; set; }
        public int OnlineUsers { get; set; } // Currently online
        
        // Content metrics
        public int TotalBlogPosts { get; set; }
        public int BlogPostsCreatedToday { get; set; }
        public int TotalComments { get; set; }
        public int CommentsPostedToday { get; set; }
        
        // System metrics
        public double AverageSessionDuration { get; set; } // In minutes
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int TotalFlags { get; set; }
        public int PendingFlags { get; set; }
        
        // Performance metrics
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}