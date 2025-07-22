using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            
            // Configure unique indexes
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
                
            entity.HasIndex(u => new { u.Provider, u.ProviderUserId })
                .IsUnique()
                .HasDatabaseName("IX_Users_Provider_ProviderUserId");
                
            // Configure column types to match PostgreSQL schema
            entity.Property(u => u.Id)
                .HasColumnType("text")
                .IsRequired();
                
            entity.Property(u => u.Name)
                .HasColumnType("character varying(255)")
                .IsRequired();
                
            entity.Property(u => u.Email)
                .HasColumnType("character varying(255)")
                .IsRequired();
                
            entity.Property(u => u.Image)
                .HasColumnType("character varying(500)")
                .IsRequired();
                
            entity.Property(u => u.Provider)
                .HasColumnType("character varying(50)")
                .IsRequired();
                
            entity.Property(u => u.ProviderUserId)
                .HasColumnType("character varying(255)")
                .IsRequired();
                
            entity.Property(u => u.Bio)
                .HasColumnType("character varying(1000)")
                .IsRequired();
                
            entity.Property(u => u.PhoneNumber)
                .HasColumnType("character varying(20)")
                .IsRequired();
                
            entity.Property(u => u.DateOfBirth)
                .HasColumnType("timestamp with time zone");
                
            entity.Property(u => u.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
                
            entity.Property(u => u.LastLoginAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
                
            entity.Property(u => u.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
