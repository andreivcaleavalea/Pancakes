using AdminService.Data;
using AdminService.Data.Seeders;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Configuration
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
                await context.Database.MigrateAsync();
                await AdminDataSeeder.SeedAsync(context);
                Console.WriteLine("Database migrations and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database setup: {ex.Message}");
                throw;
            }
        }
    }
}
