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

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load("../../.env");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? "Host=localhost;Port=5432;Database=PancakesBlogDB;Username=postgres;Password=postgres123";

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(UserService.Helpers.MappingProfile));

// Add HttpContextAccessor for current user service
builder.Services.AddHttpContextAccessor();

// Add HttpClient for OAuth service
builder.Services.AddHttpClient<OAuthService>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEducationRepository, EducationRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IHobbyRepository, HobbyRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IPersonalPageSettingsRepository, PersonalPageSettingsRepository>();

// Register Services
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IEducationService, EducationService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IHobbyService, HobbyService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPersonalPageService, PersonalPageService>();

// Auth Services (keeping existing for now)
builder.Services.AddScoped<OAuthService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<CurrentUserService>();

// Add CORS from environment variables
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
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
app.UseCors("AllowFrontend");

// Serve static files (profile pictures)
var assetsPath = Path.Combine(app.Environment.ContentRootPath, "assets");
if (!Directory.Exists(assetsPath))
{
    Directory.CreateDirectory(assetsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(assetsPath),
    RequestPath = "/assets"
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();