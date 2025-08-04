using System.Net;
using System.Text.Json;

namespace AdminService.Middleware
{
    public class IpWhitelistingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpWhitelistingMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public IpWhitelistingMiddleware(RequestDelegate next, ILogger<IpWhitelistingMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            
            if (!IsIpWhitelisted(clientIp))
            {
                _logger.LogWarning("Access denied for IP {ClientIp} on path {Path}", clientIp, context.Request.Path);

                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Access denied. Your IP address is not authorized to access this admin panel.",
                    clientIp = clientIp,
                    timestamp = DateTime.UtcNow
                }));
                return;
            }

            _logger.LogDebug("IP {ClientIp} whitelisted for path {Path}", clientIp, context.Request.Path);
            await _next(context);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP headers (load balancer/proxy scenarios)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Take the first IP in the chain (original client)
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsIpWhitelisted(string clientIp)
        {
            _logger.LogError("1 step");
            try
            {
                // Get whitelist from environment variable or configuration
                var whitelistEnv = Environment.GetEnvironmentVariable("ADMIN_IP_WHITELIST");
                var whitelistConfig = _configuration["AdminSecurity:IpWhitelist"];

                var whitelist = whitelistEnv ?? whitelistConfig ?? "";

                // If no whitelist is configured, allow all (development mode)
                if (string.IsNullOrEmpty(whitelist))
                {
                    _logger.LogWarning("No IP whitelist configured. Allowing all IPs (development mode)");
                    return true;
                }

                var allowedIps = whitelist.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ip => ip.Trim())
                    .ToList();

                // Check for exact match
                if (allowedIps.Contains(clientIp))
                {
                    return true;
                }

                // Check for localhost variants
                if (IsLocalhost(clientIp))
                {
                    return allowedIps.Any(ip => IsLocalhost(ip) || ip == "localhost" || ip == "127.0.0.1" || ip == "::1");
                }

                // Check for CIDR ranges
                foreach (var allowedIp in allowedIps)
                {
                    if (IsIpInCidrRange(clientIp, allowedIp))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking IP whitelist for {ClientIp}", clientIp);
                // Fail secure - deny access if we can't validate
                return false;
            }
        }

        private static bool IsLocalhost(string ip)
        {
            return ip == "::1" || ip == "127.0.0.1" || ip.StartsWith("127.") || ip == "localhost";
        }

        private static bool IsIpInCidrRange(string clientIp, string cidrRange)
        {
            try
            {
                // Simple CIDR check (basic implementation)
                if (!cidrRange.Contains('/'))
                {
                    return clientIp == cidrRange;
                }

                var parts = cidrRange.Split('/');
                if (parts.Length != 2 || !int.TryParse(parts[1], out var prefixLength))
                {
                    return false;
                }

                var networkAddress = IPAddress.Parse(parts[0]);
                var clientAddress = IPAddress.Parse(clientIp);

                // Only handle IPv4 for now
                if (networkAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                    clientAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return false;
                }

                var networkBytes = networkAddress.GetAddressBytes();
                var clientBytes = clientAddress.GetAddressBytes();

                var mask = CreateSubnetMask(prefixLength);
                var maskBytes = mask.GetAddressBytes();

                for (int i = 0; i < 4; i++)
                {
                    if ((networkBytes[i] & maskBytes[i]) != (clientBytes[i] & maskBytes[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static IPAddress CreateSubnetMask(int prefixLength)
        {
            if (prefixLength < 0 || prefixLength > 32)
            {
                throw new ArgumentException("Invalid prefix length");
            }

            uint mask = 0;
            for (int i = 0; i < prefixLength; i++)
            {
                mask |= (uint)(1 << (31 - i));
            }

            return new IPAddress(BitConverter.GetBytes(mask).Reverse().ToArray());
        }

        /// <summary>
        /// Check if IP whitelisting should be bypassed for development
        /// </summary>
        private bool ShouldBypassWhitelist(HttpContext context)
        {
            // Only bypass in development environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                var bypassWhitelist = Environment.GetEnvironmentVariable("BYPASS_IP_WHITELIST");
                return bypassWhitelist?.ToLower() == "true";
            }

            return false;
        }
    }
}