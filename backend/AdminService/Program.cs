using AdminService.Configuration;
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
builder.Services.AddHttpContextAccessor();

// Add application services using extension methods
builder.Services
    .AddDatabaseServices()
    .AddApplicationServices()
    .AddHttpClients()
    .AddJwtAuthentication()
    .AddAuthorizationPolicies()
    .AddCorsConfiguration();

var app = builder.Build();

// Configure application pipeline
app.ConfigureUrls()
   .ConfigurePipeline();

// Initialize database
await DatabaseInitializer.InitializeDatabaseAsync(app.Services);

// Print startup information
app.PrintStartupInfo();

app.Run();

public partial class Program {}