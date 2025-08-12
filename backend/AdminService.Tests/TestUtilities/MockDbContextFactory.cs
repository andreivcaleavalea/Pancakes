using AdminService.Data;
using AdminService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Tests.TestUtilities;

/// <summary>
/// Factory for creating in-memory database contexts for testing.
/// Provides isolated, clean database instances for each test.
/// </summary>
public static class MockDbContextFactory
{
    /// <summary>
    /// Creates a new in-memory AdminDbContext with unique database name.
    /// Each call creates a completely isolated database for testing.
    /// </summary>
    public static AdminDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AdminDbContext(options);
    }

    /// <summary>
    /// Creates a context and seeds it with basic test data.
    /// </summary>
    public static AdminDbContext CreateSeededContext()
    {
        var context = CreateInMemoryContext();
        SeedTestData(context);
        return context;
    }

    /// <summary>
    /// Seeds the context with basic test data for common scenarios.
    /// </summary>
    private static void SeedTestData(AdminDbContext context)
    {
        // Create test roles
        var superAdminRole = new AdminRole
        {
            Id = Guid.NewGuid(),
            Name = "SuperAdmin",
            Description = "Super administrator with all permissions",
            Permissions = TestDataHelper.GetTestPermissionsJson(),
            Level = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var moderatorRole = new AdminRole
        {
            Id = Guid.NewGuid(),
            Name = "Moderator", 
            Description = "Moderator with limited permissions",
            Permissions = System.Text.Json.JsonSerializer.Serialize(new List<string> { "CanViewUsers", "CanViewBlogs" }),
            Level = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.AdminRoles.AddRange(superAdminRole, moderatorRole);

        // Create test admin users
        var superAdmin = new AdminUser
        {
            Id = "admin-super",
            Email = "super@admin.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Name = "Super Admin",
            AdminLevel = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = new List<AdminRole> { superAdminRole }
        };

        var moderator = new AdminUser
        {
            Id = "admin-mod",
            Email = "mod@admin.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Name = "Mod Admin",
            AdminLevel = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = new List<AdminRole> { moderatorRole }
        };

        context.AdminUsers.AddRange(superAdmin, moderator);

        // Create some audit logs
        var auditLog = new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            AdminId = superAdmin.Id,
            Action = "Login",
            TargetType = "User",
            TargetId = superAdmin.Id,
            Details = "User logged in successfully",
            Timestamp = DateTime.UtcNow,
            IpAddress = "192.168.1.1"
        };

        context.AdminAuditLogs.Add(auditLog);

        context.SaveChanges();
    }
}