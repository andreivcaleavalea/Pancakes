using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdminService.Data
{
    public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
    {
        public AdminDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AdminDbContext>();
            
            // Load environment variables
            DotNetEnv.Env.Load("../../.env");
            
            var host = Environment.GetEnvironmentVariable("ADMIN_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("ADMIN_DB_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("ADMIN_DB_DATABASE") ?? "PancakesAdminDB";
            var username = Environment.GetEnvironmentVariable("ADMIN_DB_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("ADMIN_DB_PASSWORD") ?? "postgres123";
            
            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
            optionsBuilder.UseNpgsql(connectionString);
            
            return new AdminDbContext(optionsBuilder.Options);
        }
    }
}