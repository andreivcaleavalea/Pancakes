using Microsoft.EntityFrameworkCore;
using BlogService.Models.Entities;

namespace BlogService.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Comment> Comments { get; set; }

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

        base.OnModelCreating(modelBuilder);
    }
}
