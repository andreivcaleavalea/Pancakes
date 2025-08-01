using AdminService.Authorization;
using AdminService.Data;
using AdminService.Data.Seeders;
using AdminService.Middleware;
using AdminService.Services.Interfaces;
using AdminService.Services.Implementations;
using AdminService.Clients;
using AdminService.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

// Add Background Services
builder.Services.AddHostedService<RateLimitCleanupService>();

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

        // Configure to read JWT token from httpOnly cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Check for token in httpOnly cookie first
                if (context.Request.Cookies.TryGetValue("adminToken", out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                // Fallback to Authorization header if cookie not present
                else if (context.Request.Headers.Authorization.FirstOrDefault()?.StartsWith("Bearer ") == true)
                {
                    context.Token = context.Request.Headers.Authorization.FirstOrDefault()?.Substring("Bearer ".Length).Trim();
                }
                return Task.CompletedTask;
            }
        };
    });

// Configure Authorization with Policies
builder.Services.AddAuthorization(options =>
{
    // User Management Policies
    options.AddPolicy("CanViewUsers", policy => policy.RequirePermission(AdminPermissions.ViewUsers));
    options.AddPolicy("CanBanUsers", policy => policy.RequirePermission(AdminPermissions.BanUsers));
    options.AddPolicy("CanUnbanUsers", policy => policy.RequirePermission(AdminPermissions.UnbanUsers));
    options.AddPolicy("CanDeleteUsers", policy => policy.RequirePermission(AdminPermissions.DeleteUsers));
    options.AddPolicy("CanViewUserDetails", policy => policy.RequirePermission(AdminPermissions.ViewUserDetails));

    // Admin Management Policies  
    options.AddPolicy("CanViewAdmins", policy => policy.RequirePermission(AdminPermissions.ViewAdmins));
    options.AddPolicy("CanCreateAdmins", policy => policy.RequirePermission(AdminPermissions.CreateAdmins));
    options.AddPolicy("CanUpdateAdmins", policy => policy.RequirePermission(AdminPermissions.UpdateAdmins));
    options.AddPolicy("CanDeleteAdmins", policy => policy.RequirePermission(AdminPermissions.DeleteAdmins));
    options.AddPolicy("CanManageRoles", policy => policy.RequirePermission(AdminPermissions.ManageRoles));

    // Content Moderation Policies
    options.AddPolicy("CanViewContent", policy => policy.RequirePermission(AdminPermissions.ViewContent));
    options.AddPolicy("CanModerateContent", policy => policy.RequirePermission(AdminPermissions.ModerateContent));
    options.AddPolicy("CanDeleteContent", policy => policy.RequirePermission(AdminPermissions.DeleteContent));
    options.AddPolicy("CanViewReports", policy => policy.RequirePermission(AdminPermissions.ViewReports));

    // Analytics Policies
    options.AddPolicy("CanViewAnalytics", policy => policy.RequirePermission(AdminPermissions.ViewAnalytics));
    options.AddPolicy("CanViewDashboard", policy => policy.RequirePermission(AdminPermissions.ViewDashboard));
    options.AddPolicy("CanExportData", policy => policy.RequirePermission(AdminPermissions.ExportData));

    // System Configuration Policies
    options.AddPolicy("CanViewSystemConfig", policy => policy.RequirePermission(AdminPermissions.ViewSystemConfig));
    options.AddPolicy("CanUpdateSystemConfig", policy => policy.RequirePermission(AdminPermissions.UpdateSystemConfig));
    options.AddPolicy("CanViewLogs", policy => policy.RequirePermission(AdminPermissions.ViewLogs));
    options.AddPolicy("CanManageBackups", policy => policy.RequirePermission(AdminPermissions.ManageBackups));

    // Audit Policies
    options.AddPolicy("CanViewAuditLogs", policy => policy.RequirePermission(AdminPermissions.ViewAuditLogs));
    options.AddPolicy("CanExportAuditLogs", policy => policy.RequirePermission(AdminPermissions.ExportAuditLogs));
});

// Register authorization handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Configure CORS
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
    ?? new[] { "http://localhost:3001", "http://localhost:3002", "http://localhost:5174" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminPanel", policy =>
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS") // Specific methods instead of AllowAnyMethod
              .WithHeaders("Content-Type", "Authorization", "Accept", "X-Requested-With") // Specific headers instead of AllowAnyHeader
              .AllowCredentials()); // Required for httpOnly cookies
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

// IP whitelisting middleware (early security check)
app.UseMiddleware<IpWhitelistingMiddleware>();

// API logging middleware (first to capture all requests)
app.UseMiddleware<ApiLoggingMiddleware>();

// Input validation middleware (before authentication)
app.UseMiddleware<InputValidationMiddleware>();

// Rate limiting middleware (before authentication)
app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("=== ADMIN SERVICE STARTED ===");
Console.WriteLine($"Admin Panel URL: http://localhost:3001");
Console.WriteLine($"Admin API URL: http://localhost:{servicePort}");
Console.WriteLine("===============================");

app.Run();