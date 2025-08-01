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
            var host = Environment.GetEnvironmentVariable("USERS_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("USERS_DB_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("USERS_DB_DATABASE") ?? "PancakesBlogDB";
            var username = Environment.GetEnvironmentVariable("USERS_DB_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("USERS_DB_PASSWORD") ?? "postgres123";
            
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new UserDbContext(optionsBuilder.Options);
    }
}
