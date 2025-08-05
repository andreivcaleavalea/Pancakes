using AdminService.Middleware;

namespace AdminService.Configuration
{
    public static class WebApplicationExtensions
    {
        public static WebApplication ConfigureUrls(this WebApplication app)
        {
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

            return app;
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin Service API V1");
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAdminPanel");

            // IP whitelisting middleware (early security check)
            app.UseMiddleware<IpWhitelistingMiddleware>();

            // API logging middleware (first to capture all requests)
            app.UseMiddleware<ApiLoggingMiddleware>();

            // Rate limiting middleware (before authentication)
            app.UseMiddleware<RateLimitingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }

        public static void PrintStartupInfo(this WebApplication app)
        {
            var servicePort = Environment.GetEnvironmentVariable("ADMIN_SERVICE_PORT") ?? "5002";
            
            Console.WriteLine("=== ADMIN SERVICE STARTED ===");
            Console.WriteLine($"Admin Panel URL: http://localhost:3001");
            Console.WriteLine($"Admin API URL: http://localhost:{servicePort}");
            Console.WriteLine("===============================");
        }
    }
}
