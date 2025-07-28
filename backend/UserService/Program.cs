using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using UserService.Services;
using UserService.Services.Implementations;
using UserService.Services.Interfaces;
using UserService.Data;
using UserService.Repositories.Interfaces;
using UserService.Repositories.Implementations;
using UserService.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load("../../.env");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add HttpContextAccessor for current user service
builder.Services.AddHttpContextAccessor();

// Add HttpClient for OAuth service
builder.Services.AddHttpClient<OAuthService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Entity Framework
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
    ?? throw new InvalidOperationException("POSTGRES_CONNECTION_STRING must be set in environment variables");

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();

// Add custom services
builder.Services.AddScoped<OAuthService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<IUserService, UserService.Services.Implementations.UserService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserMappingService, UserMappingService>();

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
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying database migrations: {ex.Message}");
    }
}

Console.WriteLine($"Starting UserService on port 5141");
app.Run();