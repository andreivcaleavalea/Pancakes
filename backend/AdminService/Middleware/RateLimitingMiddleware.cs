using System.Collections.Concurrent;
using System.Net;

namespace AdminService.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, ClientRateLimit> _clients = new();

        // Rate limiting configuration
        private const int MaxRequests = 5; // Max login attempts
        private const int WindowMinutes = 15; // In 15-minute window
        private const int LockoutMinutes = 30; // Lockout duration after exceeding limit

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply rate limiting to sensitive endpoints
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && ShouldApplyRateLimit(path))
            {
                var clientId = GetClientIdentifier(context);
                var rateLimitResult = CheckRateLimit(clientId);

                if (!rateLimitResult.IsAllowed)
                {
                    _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Path}. Attempts: {Attempts}", 
                        clientId, path, rateLimitResult.AttemptCount);

                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers["Retry-After"] = ((int)rateLimitResult.RetryAfter.TotalSeconds).ToString();
                    
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = $"Too many requests. Try again in {rateLimitResult.RetryAfter.TotalMinutes:F0} minutes.",
                        retryAfterSeconds = (int)rateLimitResult.RetryAfter.TotalSeconds
                    }));
                    return;
                }

                // Track the request
                RecordRequest(clientId);
            }

            await _next(context);
        }

        private static bool ShouldApplyRateLimit(string path)
        {
            // Apply rate limiting to authentication endpoints
            return path.Contains("/adminauth/login") || 
                   path.Contains("/adminauth/bootstrap") ||
                   path.Contains("/adminauth/reset-password");
        }

        private static string GetClientIdentifier(HttpContext context)
        {
            // Use combination of IP address and User-Agent for better identification
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers.UserAgent.ToString();
            return $"{ip}:{userAgent.GetHashCode()}";
        }

        private static RateLimitResult CheckRateLimit(string clientId)
        {
            var now = DateTime.UtcNow;
            var client = _clients.GetOrAdd(clientId, _ => new ClientRateLimit());

            lock (client)
            {
                // Clean old requests outside the window
                client.Requests.RemoveAll(r => now - r > TimeSpan.FromMinutes(WindowMinutes));

                // Check if client is currently locked out
                if (client.LockoutUntil.HasValue && now < client.LockoutUntil.Value)
                {
                    return new RateLimitResult
                    {
                        IsAllowed = false,
                        AttemptCount = client.Requests.Count,
                        RetryAfter = client.LockoutUntil.Value - now
                    };
                }

                // Check if within rate limit
                if (client.Requests.Count < MaxRequests)
                {
                    return new RateLimitResult { IsAllowed = true, AttemptCount = client.Requests.Count };
                }

                // Rate limit exceeded - set lockout
                client.LockoutUntil = now.AddMinutes(LockoutMinutes);
                return new RateLimitResult
                {
                    IsAllowed = false,
                    AttemptCount = client.Requests.Count,
                    RetryAfter = TimeSpan.FromMinutes(LockoutMinutes)
                };
            }
        }

        private static void RecordRequest(string clientId)
        {
            var client = _clients.GetOrAdd(clientId, _ => new ClientRateLimit());
            lock (client)
            {
                client.Requests.Add(DateTime.UtcNow);
            }
        }

        // Clean up old client data periodically
        public static void CleanupExpiredClients()
        {
            var cutoff = DateTime.UtcNow.AddHours(-2); // Remove clients older than 2 hours
            var keysToRemove = _clients
                .Where(kvp => kvp.Value.Requests.All(r => r < cutoff) && 
                             (!kvp.Value.LockoutUntil.HasValue || kvp.Value.LockoutUntil.Value < DateTime.UtcNow))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _clients.TryRemove(key, out _);
            }
        }
    }

    public class ClientRateLimit
    {
        public List<DateTime> Requests { get; set; } = new();
        public DateTime? LockoutUntil { get; set; }
    }

    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public int AttemptCount { get; set; }
        public TimeSpan RetryAfter { get; set; }
    }
}