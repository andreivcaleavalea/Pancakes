using Microsoft.EntityFrameworkCore;
using BlogService.Models.Entities;

namespace BlogService.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<PostRating> PostRatings { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<SavedBlog> SavedBlogs { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<UserInterest> UserInterests { get; set; }
    public DbSet<PersonalizedFeed> PersonalizedFeeds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure BlogPost-Comment relationship
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.BlogPost)
            .WithMany()
            .HasForeignKey(c => c.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Comment self-referencing relationship for replies
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes for replies

        // Configure PostRating relationships
        modelBuilder.Entity<PostRating>()
            .HasOne(pr => pr.BlogPost)
            .WithMany()
            .HasForeignKey(pr => pr.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure one rating per user per post
        modelBuilder.Entity<PostRating>()
            .HasIndex(pr => new { pr.BlogPostId, pr.UserId })
            .IsUnique();

        // Configure CommentLike relationships
        modelBuilder.Entity<CommentLike>()
            .HasOne(cl => cl.Comment)
            .WithMany()
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure one like per user per comment
        modelBuilder.Entity<CommentLike>()
            .HasIndex(cl => new { cl.CommentId, cl.UserId })
            .IsUnique();

        // Configure SavedBlog relationships
        modelBuilder.Entity<SavedBlog>()
            .HasKey(sb => new { sb.UserId, sb.BlogPostId }); // Composite primary key

        modelBuilder.Entity<SavedBlog>()
            .HasOne(sb => sb.BlogPost)
            .WithMany()
            .HasForeignKey(sb => sb.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Reports table - no foreign key constraints since ContentId can reference
        // either BlogPost or Comment based on ContentType
        // Content validation is handled in the service layer

        // Add check constraint to prevent self-reporting
        modelBuilder.Entity<Report>()
            .HasCheckConstraint("CK_Report_NoSelfReport", "\"ReporterId\" != \"ReportedUserId\"");

        // Configure UserInterest entity
        UserInterest.ConfigureEntity(modelBuilder);

        // Configure PersonalizedFeed entity
        PersonalizedFeed.ConfigureEntity(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}
