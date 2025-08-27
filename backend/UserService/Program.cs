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
    options.UseNpgsql(connectionString));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(UserService.Helpers.MappingProfile));

// Add HttpContextAccessor for current user service
builder.Services.AddHttpContextAccessor();

// Add HttpClient for OAuth service
builder.Services.AddHttpClient<OAuthService>();



// Add repositories
// Add Memory Caching for performance optimization  
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBanRepository, BanRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IEducationRepository, EducationRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IHobbyRepository, HobbyRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IPersonalPageSettingsRepository, PersonalPageSettingsRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Add custom services
builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IBanService, BanService>();
builder.Services.AddScoped<IUserService, UserService.Services.Implementations.UserService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserMappingService, UserMappingService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IEducationService, EducationService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IHobbyService, HobbyService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPersonalPageService, PersonalPageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add profile picture strategy services
builder.Services.AddScoped<IProfilePictureStrategy, UserService.Services.Implementations.ProfilePictureStrategies.OAuthProfilePictureStrategy>();
builder.Services.AddScoped<IProfilePictureStrategy, UserService.Services.Implementations.ProfilePictureStrategies.SelfProvidedProfilePictureStrategy>();
builder.Services.AddScoped<IProfilePictureStrategyFactory, UserService.Services.Implementations.ProfilePictureStrategies.ProfilePictureStrategyFactory>();

// Add CORS from environment variables
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

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

// Add JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? throw new InvalidOperationException("JWT_SECRET must be set in environment variables");

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Configure static file serving for profile pictures
var assetsPath = Path.Combine(builder.Environment.ContentRootPath, "assets");
if (!Directory.Exists(assetsPath))
{
    Directory.CreateDirectory(assetsPath);
    Directory.CreateDirectory(Path.Combine(assetsPath, "profile-pictures"));
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(assetsPath),
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

Console.WriteLine($"Starting UserService on port 5141");
app.Run();