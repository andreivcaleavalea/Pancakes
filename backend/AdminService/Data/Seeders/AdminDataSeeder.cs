using AdminService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AdminService.Data.Seeders
{
    public static class AdminDataSeeder
    {
        public static async Task SeedAsync(AdminDbContext context)
        {
            try
            {
                // Seed default admin roles
                await SeedAdminRoles(context);
                
                // Seed default super admin user
                await SeedDefaultSuperAdmin(context);

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding admin data: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedAdminRoles(AdminDbContext context)
        {
            if (await context.AdminRoles.AnyAsync())
                return;

            var roles = new List<AdminRole>
            {
                new AdminRole
                {
                    Name = "SuperAdmin",
                    Description = "Full system access with all permissions",
                    Level = 4,
                    Permissions = JsonSerializer.Serialize(new[]
                    {
                        "users:view", "users:ban", "users:unban", "users:delete", "users:details",
                        "admins:view", "admins:create", "admins:update", "admins:delete", "admins:roles",
                        "content:view", "content:moderate", "content:delete", "content:reports",
                        "analytics:view", "analytics:dashboard", "analytics:export",
                        "system:view", "system:update", "system:logs", "system:backups",
                        "audit:view", "audit:export"
                    })
                },
                new AdminRole
                {
                    Name = "SystemAdmin",
                    Description = "System administration and user management",
                    Level = 3,
                    Permissions = JsonSerializer.Serialize(new[]
                    {
                        "users:view", "users:ban", "users:unban", "users:details",
                        "content:view", "content:moderate", "content:delete",
                        "system:logs", "analytics:view"
                    })
                },
                new AdminRole
                {
                    Name = "ContentModerator",
                    Description = "Content moderation and community management",
                    Level = 2,
                    Permissions = JsonSerializer.Serialize(new[]
                    {
                        "users:view", "content:view", "content:moderate", "content:delete", "content:reports"
                    })
                },
                new AdminRole
                {
                    Name = "SupportAgent",
                    Description = "Basic support and user assistance",
                    Level = 1,
                    Permissions = JsonSerializer.Serialize(new[]
                    {
                        "users:view", "content:view"
                    })
                }
            };

            await context.AdminRoles.AddRangeAsync(roles);
        }

        private static async Task SeedDefaultSuperAdmin(AdminDbContext context)
        {
            if (await context.AdminUsers.AnyAsync())
                return;

            var defaultPassword = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD") ?? "AdminPassword123!";
            var passwordHash = HashPassword(defaultPassword);

            var superAdminRole = await context.AdminRoles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            if (superAdminRole == null)
            {
                Console.WriteLine("SuperAdmin role not found, cannot create default admin user");
                return;
            }

            var superAdmin = new AdminUser
            {
                Email = "admin@pancakes.com",
                Name = "Super Administrator",
                PasswordHash = passwordHash,
                AdminLevel = 4,
                IsActive = true,
                RequirePasswordChange = true,
                TwoFactorEnabled = false,
                Roles = new List<AdminRole> { superAdminRole }
            };

            await context.AdminUsers.AddAsync(superAdmin);

            Console.WriteLine("=== DEFAULT SUPER ADMIN CREATED ===");
            Console.WriteLine($"Email: {superAdmin.Email}");
            Console.WriteLine($"Password: {defaultPassword}");
            Console.WriteLine("Please change the password immediately after first login!");
            Console.WriteLine("=====================================");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var salt = Guid.NewGuid().ToString();
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var hashedPassword = Convert.ToBase64String(hashedBytes);
            return $"{hashedPassword}:{salt}";
        }
    }
}