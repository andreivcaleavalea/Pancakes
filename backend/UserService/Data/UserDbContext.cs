using Microsoft.EntityFrameworkCore;
using UserService.Models.Entities;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Education> Educations { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Hobby> Hobbies { get; set; }
    public DbSet<PersonalPageSettings> PersonalPageSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.ProviderUserId)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Bio)
                .HasMaxLength(1000);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Image)
                .HasMaxLength(500);
        });

        // Configure Friendship entity
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.SenderId)
                .IsRequired();

            entity.Property(e => e.ReceiverId)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Create unique index to prevent duplicate friendships
            entity.HasIndex(e => new { e.SenderId, e.ReceiverId })
                .IsUnique();

            // Add constraint to prevent self-friendship
            entity.HasCheckConstraint("CK_Friendship_NoSelfFriend", "\"SenderId\" != \"ReceiverId\"");
        });
    }
}
