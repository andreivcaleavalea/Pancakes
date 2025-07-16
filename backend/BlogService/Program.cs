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

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<BlogDbContext>(options =>
{
    var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
    var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
    var database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? "PancakesBlogDB";
    var username = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres";
    var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres123";
    
    var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    options.UseNpgsql(connectionString);
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();

builder.Services.AddScoped<IBlogPostService, BlogPostService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment() && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    app.Urls.Clear();
    app.Urls.Add("http://0.0.0.0:80");
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
