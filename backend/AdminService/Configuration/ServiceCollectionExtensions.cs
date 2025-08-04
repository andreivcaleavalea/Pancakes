using AdminService.Authorization;
using AdminService.Data;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AdminService.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
        {
            var host = Environment.GetEnvironmentVariable("ADMIN_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("ADMIN_DB_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("ADMIN_DB_DATABASE") ?? "PancakesAdminDB";
            var username = Environment.GetEnvironmentVariable("ADMIN_DB_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("ADMIN_DB_PASSWORD") ?? "postgres123";

            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

            services.AddDbContext<AdminDbContext>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Add AutoMapper
            services.AddAutoMapper(typeof(Program));

            // Add Services
            services.AddScoped<IAdminAuthService, AdminService.Services.Implementations.AdminAuthService>();
            services.AddScoped<IAuditService, AdminService.Services.Implementations.AuditService>();
            services.AddScoped<IAnalyticsService, AdminService.Services.Implementations.AnalyticsService>();
            services.AddScoped<IServiceJwtService, AdminService.Services.Implementations.ServiceJwtService>();

            // Add Background Services
            services.AddHostedService<AdminService.Services.Implementations.RateLimitCleanupService>();

            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient<AdminService.Clients.UserClient.UserServiceClient>();
            services.AddScoped<AdminService.Clients.UserClient.UserServiceClient>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
        {
            var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesAdmin";
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesAdminUsers";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // User Management Policies
                options.AddPolicy("CanViewUsers", policy => policy.RequirePermission(AdminPermissions.ViewUsers));
                options.AddPolicy("CanBanUsers", policy => policy.RequirePermission(AdminPermissions.BanUsers));
                options.AddPolicy("CanUnbanUsers", policy => policy.RequirePermission(AdminPermissions.UnbanUsers));
                options.AddPolicy("CanViewUserDetails", policy => policy.RequirePermission(AdminPermissions.ViewUserDetails));
                options.AddPolicy("CanUpdateUsers", policy => policy.RequirePermission(AdminPermissions.UpdateUsers));

                // Analytics Policies
                options.AddPolicy("CanViewAnalytics", policy => policy.RequirePermission(AdminPermissions.ViewAnalytics));
                options.AddPolicy("CanViewDashboard", policy => policy.RequirePermission(AdminPermissions.ViewDashboard));

                // Audit Policies
                options.AddPolicy("CanViewAuditLogs", policy => policy.RequirePermission(AdminPermissions.ViewAuditLogs));
            });

            // Register authorization handler
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
                ?? new[] { "http://localhost:3001", "http://localhost:3002", "http://localhost:5174" };

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAdminPanel", policy =>
                    policy.WithOrigins(allowedOrigins)
                          .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS") 
                          .WithHeaders("Content-Type", "Authorization", "Accept", "X-Requested-With") 
                          .AllowCredentials()); // Required for httpOnly cookies
            });

            return services;
        }
    }
}
