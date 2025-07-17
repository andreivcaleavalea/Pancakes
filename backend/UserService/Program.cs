using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add HttpContextAccessor for current user service
builder.Services.AddHttpContextAccessor();

// Add HttpClient for OAuth service
builder.Services.AddHttpClient<OAuthService>();

// Add custom services
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();