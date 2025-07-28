using BlogService.Data;
using BlogService.Data.Seeders;
using BlogService.Repositories.Interfaces;
using BlogService.Repositories.Implementations;
using BlogService.Services.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Helpers;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using AutoMapper;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });
builder.Services.AddOpenApi();

// Add HttpContextAccessor for accessing HTTP context in services
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<BlogDbContext>(options =>
{
    var host = Environment.GetEnvironmentVariable("BLOGS_DB_HOST");
    var port = Environment.GetEnvironmentVariable("BLOGS_DB_PORT");
    var database = Environment.GetEnvironmentVariable("BLOGS_DB_DATABASE");
    var username = Environment.GetEnvironmentVariable("BLOGS_DB_USERNAME");
    var password = Environment.GetEnvironmentVariable("BLOGS_DB_PASSWORD");
    
    var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    options.UseNpgsql(connectionString);
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPostRatingRepository, PostRatingRepository>();
builder.Services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();
builder.Services.AddScoped<ISavedBlogRepository, SavedBlogRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();

builder.Services.AddScoped<IBlogPostService, BlogPostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IPostRatingService, PostRatingService>();
builder.Services.AddScoped<ICommentLikeService, CommentLikeService>();
builder.Services.AddScoped<ISavedBlogService, SavedBlogService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();

// Add JWT User Service for extracting user info from tokens
builder.Services.AddScoped<IJwtUserService, JwtUserService>();

// Add new authorization and context services
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IModelValidationService, ModelValidationService>();
builder.Services.AddScoped<IFriendsPostService, FriendsPostService>();

// Add HttpClient for UserService communication
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>();
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure JWT Authentication
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure CORS from environment variables
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') 
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// Configure port based on environment variable
var port = Environment.GetEnvironmentVariable("BLOG_SERVICE_PORT") ?? "5001";
System.Console.WriteLine($"Blog Service running on port: {port}");
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    app.Urls.Clear();
    app.Urls.Add("http://0.0.0.0:80");
}
else
{
    app.Urls.Clear();
    app.Urls.Add($"http://localhost:{port}");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Blog Service API V1");
    });
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    await context.Database.MigrateAsync();
    
    if (app.Environment.IsDevelopment())
    {
        await BlogDataSeeder.SeedAsync(context);
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    await context.Database.MigrateAsync();
    
    await BlogDataSeeder.SeedAsync(context);
}

app.Run();
