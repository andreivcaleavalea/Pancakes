using Microsoft.EntityFrameworkCore;
using AdminService.Models.Entities;

namespace AdminService.Data
{
    public class AdminDbContext : DbContext
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
        {
        }

        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<AdminRole> AdminRoles { get; set; }
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<ContentFlag> ContentFlags { get; set; }
        public DbSet<SystemMetric> SystemMetrics { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AdminUser entity
            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(450)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasMany(e => e.AuditLogs)
                    .WithOne(e => e.AdminUser)
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Many-to-many relationship with AdminRoles
                entity.HasMany(e => e.Roles)
                    .WithMany(e => e.AdminUsers)
                    .UsingEntity<Dictionary<string, object>>(
                        "AdminUserRoles",
                        j => j.HasOne<AdminRole>().WithMany().HasForeignKey("AdminRoleId"),
                        j => j.HasOne<AdminUser>().WithMany().HasForeignKey("AdminUserId"));
            });

            // Configure AdminRole entity
            modelBuilder.Entity<AdminRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Permissions)
                    .HasColumnType("jsonb"); // PostgreSQL specific
            });

            // Configure AdminAuditLog entity
            modelBuilder.Entity<AdminAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AdminId)
                    .HasMaxLength(450)
                    .IsRequired();

                entity.Property(e => e.Action)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TargetType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.TargetId)
                    .HasMaxLength(450)
                    .IsRequired();

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45);

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500);

                entity.Property(e => e.Details)
                    .HasColumnType("jsonb"); // PostgreSQL specific

                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.AdminId);
                entity.HasIndex(e => new { e.TargetType, e.TargetId });
            });

            // Configure UserReport entity
            modelBuilder.Entity<UserReport>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ReportedUserId)
                    .HasMaxLength(450)
                    .IsRequired();

                entity.Property(e => e.ReporterUserId)
                    .HasMaxLength(450);

                entity.Property(e => e.Reason)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue("pending");

                entity.Property(e => e.ReviewedBy)
                    .HasMaxLength(450);

                entity.Property(e => e.ReviewNotes)
                    .HasMaxLength(500);

                entity.HasIndex(e => e.ReportedUserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure ContentFlag entity
            modelBuilder.Entity<ContentFlag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ContentType)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.ContentId)
                    .HasMaxLength(450)
                    .IsRequired();

                entity.Property(e => e.FlagType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.FlaggedBy)
                    .HasMaxLength(450);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue("pending");

                entity.Property(e => e.ReviewedBy)
                    .HasMaxLength(450);

                entity.Property(e => e.ReviewNotes)
                    .HasMaxLength(500);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.HasIndex(e => new { e.ContentType, e.ContentId });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.FlagType);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure SystemMetric entity
            modelBuilder.Entity<SystemMetric>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Timestamp);

                entity.Property(e => e.CpuUsage)
                    .HasPrecision(5, 2);

                entity.Property(e => e.MemoryUsage)
                    .HasPrecision(5, 2);

                entity.Property(e => e.DiskUsage)
                    .HasPrecision(5, 2);

                entity.Property(e => e.AverageSessionDuration)
                    .HasPrecision(10, 2);
            });

            // Configure SystemConfiguration entity
            modelBuilder.Entity<SystemConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();

                entity.Property(e => e.Key)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Category)
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.DataType)
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue("string");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(450);

                entity.HasIndex(e => e.Category);
            });
        }
    }
}