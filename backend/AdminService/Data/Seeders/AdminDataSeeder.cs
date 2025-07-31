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
                
                // Seed default system configurations
                await SeedSystemConfigurations(context);

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
                        "user.*", "content.*", "system.*", "admin.*", "analytics.*"
                    })
                },
                new AdminRole
                {
                    Name = "SystemAdmin",
                    Description = "System administration and user management",
                    Level = 3,
                    Permissions = JsonSerializer.Serialize(new[]
                    {
                        "user.view", "user.edit", "user.ban", "content.*", "system.logs", "system.analytics"
                    })
                },
                new AdminRole
                {
                    Name = "ContentModerator",
                    Description = "Content moderation and community management",
                    Level = 2,
                    Permissions = JsonSerializer.Serialize(new[]
                    {
                        "user.view", "content.view", "content.edit", "content.delete", "content.moderate"
                    })
                },
                new AdminRole
                {
                    Name = "SupportAgent",
                    Description = "Basic support and user assistance",
                    Level = 1,
                    Permissions = JsonSerializer.Serialize(new[]
                    {
                        "user.view", "content.view"
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

        private static async Task SeedSystemConfigurations(AdminDbContext context)
        {
            if (await context.SystemConfigurations.AnyAsync())
                return;

            var configs = new List<SystemConfiguration>
            {
                new SystemConfiguration
                {
                    Key = "MAINTENANCE_MODE",
                    Value = "false",
                    Category = "system",
                    Description = "Enable/disable maintenance mode for the entire platform",
                    DataType = "bool"
                },
                new SystemConfiguration
                {
                    Key = "USER_REGISTRATION_ENABLED",
                    Value = "true",
                    Category = "features",
                    Description = "Allow new user registrations",
                    DataType = "bool"
                },
                new SystemConfiguration
                {
                    Key = "MAX_BLOG_POSTS_PER_DAY",
                    Value = "5",
                    Category = "limits",
                    Description = "Maximum blog posts a user can create per day",
                    DataType = "int"
                },
                new SystemConfiguration
                {
                    Key = "MAX_COMMENTS_PER_HOUR",
                    Value = "20",
                    Category = "limits",
                    Description = "Maximum comments a user can post per hour",
                    DataType = "int"
                },
                new SystemConfiguration
                {
                    Key = "AUTO_MODERATION_ENABLED",
                    Value = "true",
                    Category = "moderation",
                    Description = "Enable automatic content moderation",
                    DataType = "bool"
                },
                new SystemConfiguration
                {
                    Key = "SPAM_DETECTION_THRESHOLD",
                    Value = "0.8",
                    Category = "moderation",
                    Description = "Spam detection confidence threshold (0.0-1.0)",
                    DataType = "decimal"
                },
                new SystemConfiguration
                {
                    Key = "SESSION_TIMEOUT_MINUTES",
                    Value = "60",
                    Category = "security",
                    Description = "User session timeout in minutes",
                    DataType = "int"
                },
                new SystemConfiguration
                {
                    Key = "ADMIN_SESSION_TIMEOUT_MINUTES",
                    Value = "30",
                    Category = "security",
                    Description = "Admin session timeout in minutes",
                    DataType = "int"
                },
                new SystemConfiguration
                {
                    Key = "BACKUP_RETENTION_DAYS",
                    Value = "30",
                    Category = "system",
                    Description = "Number of days to keep database backups",
                    DataType = "int"
                },
                new SystemConfiguration
                {
                    Key = "LOG_RETENTION_DAYS",
                    Value = "90",
                    Category = "system",
                    Description = "Number of days to keep audit logs",
                    DataType = "int"
                }
            };

            await context.SystemConfigurations.AddRangeAsync(configs);
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