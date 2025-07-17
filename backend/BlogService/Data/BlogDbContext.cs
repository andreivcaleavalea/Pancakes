using Microsoft.EntityFrameworkCore;
using BlogService.Models.Entities;

namespace BlogService.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

    public DbSet<BlogPost> BlogPosts { get; set; }
}
