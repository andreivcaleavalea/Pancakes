using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AdminService.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];

            // Log request
            await LogRequestAsync(context, requestId);

            // Capture response
            var originalResponseBody = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{RequestId}] Unhandled exception in API pipeline", requestId);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Log response
                await LogResponseAsync(context, requestId, responseBody, stopwatch.ElapsedMilliseconds);

                // Copy response back to original stream
                responseBody.Position = 0;
                await responseBody.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId)
        {
            var request = context.Request;
            var clientInfo = GetClientInfo(context);
            var userInfo = GetUserInfo(context);

            // Read request body for POST/PUT requests
            string requestBody = "";
            if ((request.Method == HttpMethods.Post || request.Method == HttpMethods.Put) &&
                request.ContentType?.Contains("application/json") == true)
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadExactlyAsync(buffer, 0, buffer.Length);
                requestBody = Encoding.UTF8.GetString(buffer);
                request.Body.Position = 0;

                // Sanitize sensitive data in logs
                requestBody = SanitizeRequestBody(requestBody, request.Path);
            }

            var logData = new
            {
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                Type = "Request",
                Method = request.Method,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                ClientInfo = clientInfo,
                UserInfo = userInfo,
                RequestBody = requestBody,
                Headers = GetSafeHeaders(request.Headers),
                ContentType = request.ContentType,
                ContentLength = request.ContentLength
            };

            // Log with different levels based on path sensitivity
            if (IsSensitiveEndpoint(request.Path))
            {
                _logger.LogWarning("[{RequestId}] API Request: {LogData}", requestId, JsonSerializer.Serialize(logData));
            }
            else
            {
                _logger.LogInformation("[{RequestId}] API Request: {Method} {Path} from {ClientIp}", 
                    requestId, request.Method, request.Path, clientInfo.IpAddress);
            }
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, MemoryStream responseBody, long elapsedMs)
        {
            var response = context.Response;
            
            // Read response body
            responseBody.Position = 0;
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Position = 0;

            var logData = new
            {
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                Type = "Response",
                StatusCode = response.StatusCode,
                ElapsedMs = elapsedMs,
                ResponseBody = SanitizeResponseBody(responseBodyText),
                Headers = GetSafeHeaders(response.Headers),
                ContentType = response.ContentType,
                ContentLength = responseBody.Length
            };

            // Log with different levels based on status code
            if (response.StatusCode >= 400)
            {
                if (response.StatusCode >= 500)
                {
                    _logger.LogError("[{RequestId}] API Response: {LogData}", requestId, JsonSerializer.Serialize(logData));
                }
                else
                {
                    _logger.LogWarning("[{RequestId}] API Response: {StatusCode} in {ElapsedMs}ms", 
                        requestId, response.StatusCode, elapsedMs);
                }
            }
            else
            {
                _logger.LogInformation("[{RequestId}] API Response: {StatusCode} in {ElapsedMs}ms", 
                    requestId, response.StatusCode, elapsedMs);
            }

            // Log slow requests
            if (elapsedMs > 5000) // 5 seconds
            {
                _logger.LogWarning("[{RequestId}] Slow API response: {ElapsedMs}ms for {Path}", 
                    requestId, elapsedMs, context.Request.Path);
            }
        }

        private static ClientInfo GetClientInfo(HttpContext context)
        {
            var request = context.Request;
            var connection = context.Connection;

            return new ClientInfo
            {
                IpAddress = GetClientIpAddress(context),
                UserAgent = request.Headers.UserAgent.ToString(),
                Referer = request.Headers.Referer.ToString(),
                AcceptLanguage = request.Headers.AcceptLanguage.ToString(),
                Port = connection.RemotePort.ToString()
            };
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP headers (load balancer/proxy scenarios)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static UserInfo GetUserInfo(HttpContext context)
        {
            var user = context.User;
            if (!user.Identity?.IsAuthenticated == true)
            {
                return new UserInfo { IsAuthenticated = false };
            }

            return new UserInfo
            {
                IsAuthenticated = true,
                UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
                Email = user.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Name = user.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
                AdminLevel = user.FindFirst("AdminLevel")?.Value ?? ""
            };
        }

        private static Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
        {
            var safeHeaders = new Dictionary<string, string>();
            var excludeHeaders = new[] { "authorization", "cookie", "x-api-key", "x-auth-token" };

            foreach (var header in headers)
            {
                if (!excludeHeaders.Contains(header.Key.ToLower()))
                {
                    safeHeaders[header.Key] = header.Value.ToString();
                }
            }

            return safeHeaders;
        }

        private static string SanitizeRequestBody(string body, PathString path)
        {
            if (string.IsNullOrEmpty(body)) return body;

            // Remove sensitive data from login requests
            if (path.Value?.Contains("/adminauth/login") == true)
            {
                try
                {
                    var json = JsonDocument.Parse(body);
                    var root = json.RootElement;
                    var sanitized = new Dictionary<string, object>();

                    foreach (var property in root.EnumerateObject())
                    {
                        if (property.Name.ToLower() == "password")
                        {
                            sanitized[property.Name] = "***REDACTED***";
                        }
                        else
                        {
                            sanitized[property.Name] = property.Value.ToString();
                        }
                    }

                    return JsonSerializer.Serialize(sanitized);
                }
                catch
                {
                    return "***INVALID_JSON***";
                }
            }

            // Limit body size in logs
            return body.Length > 1000 ? body[..1000] + "..." : body;
        }

        private static string SanitizeResponseBody(string body)
        {
            if (string.IsNullOrEmpty(body)) return body;

            // Don't log large response bodies
            if (body.Length > 2000)
            {
                return $"***LARGE_RESPONSE_{body.Length}_BYTES***";
            }

            // Try to remove token from login responses
            try
            {
                if (body.Contains("\"token\""))
                {
                    var json = JsonDocument.Parse(body);
                    // Just indicate that response was sanitized for token
                    return "***LOGIN_RESPONSE_WITH_TOKEN***";
                }
            }
            catch
            {
                // If we can't parse, just return truncated version
            }

            return body;
        }

        private static bool IsSensitiveEndpoint(PathString path)
        {
            var sensitiveEndpoints = new[]
            {
                "/adminauth/login",
                "/adminauth/bootstrap",
                "/adminauth/reset-password",
                "/usermanagement/ban",
                "/usermanagement/unban"
            };

            return sensitiveEndpoints.Any(endpoint => 
                path.Value?.Contains(endpoint, StringComparison.OrdinalIgnoreCase) == true);
        }
    }

    public class ClientInfo
    {
        public string IpAddress { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public string Referer { get; set; } = "";
        public string AcceptLanguage { get; set; } = "";
        public string Port { get; set; } = "";
    }

    public class UserInfo
    {
        public bool IsAuthenticated { get; set; }
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string AdminLevel { get; set; } = "";
    }
}