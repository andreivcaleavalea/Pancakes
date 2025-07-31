using AdminService.Data;
using AdminService.Data.Seeders;
using AdminService.Services.Interfaces;
using AdminService.Services.Implementations;
using AdminService.Clients;
using AdminService.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
DotNetEnv.Env.Load("../../.env");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Database Context
var host = Environment.GetEnvironmentVariable("ADMIN_DB_HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("ADMIN_DB_PORT") ?? "5432";
var database = Environment.GetEnvironmentVariable("ADMIN_DB_DATABASE") ?? "PancakesAdminDB";
var username = Environment.GetEnvironmentVariable("ADMIN_DB_USERNAME") ?? "postgres";
var password = Environment.GetEnvironmentVariable("ADMIN_DB_PASSWORD") ?? "postgres123";

var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Services
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IContentModerationService, ContentModerationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
builder.Services.AddScoped<IServiceJwtService, ServiceJwtService>();

// Add HTTP Clients
builder.Services.AddHttpClient<UserServiceClient>();
builder.Services.AddHttpClient<BlogServiceClient>();
builder.Services.AddScoped<UserServiceClient>();
builder.Services.AddScoped<BlogServiceClient>();

// Configure JWT Authentication
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesAdmin";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesAdminUsers";

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configure CORS
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
    ?? new[] { "http://localhost:3001", "http://localhost:3002", "http://localhost:5174" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminPanel", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// Configure port
var servicePort = Environment.GetEnvironmentVariable("ADMIN_SERVICE_PORT") ?? "5002";
Console.WriteLine($"Admin Service running on port: {servicePort}");

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    app.Urls.Clear();
    app.Urls.Add("http://0.0.0.0:80");
}
else
{
    app.Urls.Clear();
    app.Urls.Add($"http://localhost:{servicePort}");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin Service API V1");
    });
}

// Apply database migrations and seed data
using (var scope = app.Services.CreateScope())
{
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

app.UseHttpsRedirection();
app.UseCors("AllowAdminPanel");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("=== ADMIN SERVICE STARTED ===");
Console.WriteLine($"Admin Panel URL: http://localhost:3001");
Console.WriteLine($"Admin API URL: http://localhost:{servicePort}");
Console.WriteLine("===============================");

app.Run();