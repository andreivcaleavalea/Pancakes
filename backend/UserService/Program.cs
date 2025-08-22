using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using System.Text;
using UserService.Data;
using UserService.Services;
using UserService.Repositories.Interfaces;
using UserService.Repositories.Implementations;
using UserService.Services.Interfaces;
using UserService.Services.Implementations;
using UserService.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load("../../.env");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrEmpty(connectionString))
{
    var host = Environment.GetEnvironmentVariable("USERS_DB_HOST") ?? "localhost";
    var dbPort = Environment.GetEnvironmentVariable("USERS_DB_PORT") ?? "5432";
    var database = Environment.GetEnvironmentVariable("USERS_DB_DATABASE") ?? "PancakesBlogDB";
    var username = Environment.GetEnvironmentVariable("USERS_DB_USERNAME") ?? "postgres";
    var password = Environment.GetEnvironmentVariable("USERS_DB_PASSWORD") ?? "postgres123";
    
    connectionString = $"Host={host};Port={dbPort};Database={database};Username={username};Password={password}";
}

builder.Services.AddDbContext<UserDbContext>(options =>
    {
        // Optional SSL configuration for Azure PostgreSQL
        var dbSslMode = Environment.GetEnvironmentVariable("DB_SSL_MODE");
        if (!string.IsNullOrEmpty(dbSslMode))
        {
            var trust = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE") ?? "true";
            connectionString += $";Ssl Mode={dbSslMode};Trust Server Certificate={trust}";
        }
        options.UseNpgsql(connectionString);
    });

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(UserService.Helpers.MappingProfile));

// Add HttpContextAccessor for current user service
builder.Services.AddHttpContextAccessor();

// Add HttpClient for OAuth service
builder.Services.AddHttpClient<OAuthService>();


// Add CORS from environment variables
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
    ?? new[] { Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add JWT Authentication from environment variables
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? throw new InvalidOperationException("JWT_SECRET_KEY must be set in environment variables");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesBlog";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesBlogUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

var app = builder.Build();

// Configure port based on environment variable
var port = Environment.GetEnvironmentVariable("USER_SERVICE_PORT") ?? "5141";
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    // Running in container, listen on all interfaces
    app.Urls.Clear();
    app.Urls.Add("http://0.0.0.0:80");
}
else if (app.Environment.IsDevelopment())
{
    // Running locally for development
    app.Urls.Clear();
    app.Urls.Add($"http://localhost:{port}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Ensure static assets directory exists (required in container)
var assetsRoot = Path.Combine(builder.Environment.ContentRootPath, "assets");
var profilePicturesDir = Path.Combine(assetsRoot, "profile-pictures");
Directory.CreateDirectory(profilePicturesDir);

// Configure static file serving for profile pictures
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(assetsRoot),
    RequestPath = "/assets"
});

// Use CORS before authentication
app.UseCors("AllowSpecificOrigins");

// Use authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        Console.WriteLine($"Attempting to migrate database: {connectionString}");
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying database migrations: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        Console.WriteLine($"Connection string: {connectionString}");
    }
}

var userUrls = string.Join(", ", app.Urls);
Console.WriteLine($"Starting UserService. Listening on: {userUrls}");
app.Run();