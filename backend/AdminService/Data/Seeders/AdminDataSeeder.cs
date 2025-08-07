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
            var rolesToUpdate = new Dictionary<string, string[]>
            {
                ["SuperAdmin"] = new[]
                {
                    "users:view", "users:ban", "users:unban", "users:delete", "users:details", "users:update",
                    "blogs:view", "blogs:details", "blogs:manage", "blogs:delete",
                    "admins:view", "admins:create", "admins:update", "admins:delete", "admins:roles",
                    "content:view", "content:moderate", "content:delete", "content:reports",
                    "analytics:view", "analytics:dashboard", "analytics:export",
                    "system:view", "system:update", "system:logs", "system:backups",
                    "audit:view", "audit:export"
                },
                ["SystemAdmin"] = new[]
                {
                    "users:view", "users:ban", "users:unban", "users:details", "users:update",
                    "blogs:view", "blogs:details", "blogs:manage",
                    "content:view", "content:moderate", "content:delete",
                    "system:logs", "analytics:view"
                },
                ["ContentModerator"] = new[]
                {
                    "users:view", "blogs:view", "blogs:details", "blogs:manage", 
                    "content:view", "content:moderate", "content:delete", "content:reports"
                },
                ["SupportAgent"] = new[]
                {
                    "users:view", "blogs:view", "content:view"
                }
            };

            // Check if roles exist and update them, or create new ones
            foreach (var roleConfig in rolesToUpdate)
            {
                var existingRole = await context.AdminRoles.FirstOrDefaultAsync(r => r.Name == roleConfig.Key);
                
                if (existingRole != null)
                {
                    // Update existing role permissions
                    existingRole.Permissions = JsonSerializer.Serialize(roleConfig.Value);
                    existingRole.IsActive = true;
                    context.AdminRoles.Update(existingRole);
                    Console.WriteLine($"Updated permissions for role: {roleConfig.Key}");
                }
                else
                {
                    // Create new role
                    var newRole = new AdminRole
                    {
                        Name = roleConfig.Key,
                        Description = GetRoleDescription(roleConfig.Key),
                        Level = GetRoleLevel(roleConfig.Key),
                        Permissions = JsonSerializer.Serialize(roleConfig.Value),
                        IsActive = true
                    };
                    await context.AdminRoles.AddAsync(newRole);
                    Console.WriteLine($"Created new role: {roleConfig.Key}");
                }
            }
        }

        private static string GetRoleDescription(string roleName)
        {
            return roleName switch
            {
                "SuperAdmin" => "Full system access with all permissions",
                "SystemAdmin" => "System administration and user management",
                "ContentModerator" => "Content moderation and community management", 
                "SupportAgent" => "Basic support and user assistance",
                _ => "Custom admin role"
            };
        }

        private static int GetRoleLevel(string roleName)
        {
            return roleName switch
            {
                "SuperAdmin" => 4,
                "SystemAdmin" => 3,
                "ContentModerator" => 2,
                "SupportAgent" => 1,
                _ => 1
            };
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