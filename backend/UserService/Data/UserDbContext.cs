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
    public DbSet<Ban> Bans { get; set; }

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
        
        // Configure Ban entity
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .IsRequired();
                
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .IsRequired();
                
            entity.Property(e => e.Reason)
                .HasMaxLength(1000)
                .IsRequired();
                
            entity.Property(e => e.BannedBy)
                .HasMaxLength(255)
                .IsRequired();
                
            entity.Property(e => e.UnbannedBy)
                .HasMaxLength(255);
                
            entity.Property(e => e.UnbanReason)
                .HasMaxLength(500);
                
            entity.Property(e => e.BannedAt)
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .IsRequired();
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired();
            
            // Configure relationship with User
            entity.HasOne(e => e.User)
                .WithMany(u => u.Bans)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Add indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsActive });
            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}
