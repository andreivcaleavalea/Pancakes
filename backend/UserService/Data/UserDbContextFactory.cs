using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Data;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        // Load environment variables from .env file
        DotNetEnv.Env.Load("../../.env");
        
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback connection string for design time
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? "pancakes";
            var username = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password";
            
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new UserDbContext(optionsBuilder.Options);
    }
}
