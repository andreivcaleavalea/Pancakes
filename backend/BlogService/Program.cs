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

DotNetEnv.Env.Load("../../.env");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

builder.Services.AddScoped<IBlogPostService, BlogPostService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
    app.Urls.Add("http://0.0.0.0:5001");
}
else 
{
    app.Urls.Clear();
    app.Urls.Add($"http://localhost:5001");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Blog Service API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
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
