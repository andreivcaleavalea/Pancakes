using AdminService.Models.Requests;
using AdminService.Models.Responses;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AdminService.Middleware
{
    public class InputValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InputValidationMiddleware> _logger;

        public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only validate POST/PUT requests with body content
            if ((context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put) &&
                context.Request.ContentType?.Contains("application/json") == true)
            {
                var validationResult = await ValidateRequestAsync(context);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Input validation failed for {Path}: {Errors}", 
                        context.Request.Path, string.Join(", ", validationResult.Errors));

                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Input validation failed",
                        Errors = validationResult.Errors.ToList()
                    }));
                    return;
                }
            }

            await _next(context);
        }

        private static async Task<ValidationResult> ValidateRequestAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            if (path == null) return new ValidationResult { IsValid = true };

            // Read request body
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (string.IsNullOrEmpty(body))
                return new ValidationResult { IsValid = true };

            try
            {
                // Validate based on endpoint
                return path switch
                {
                    var p when p.Contains("/adminauth/login") => ValidateLoginRequest(body),
                    var p when p.Contains("/adminauth/bootstrap") => ValidateCreateAdminRequest(body),
                    var p when p.Contains("/usermanagement/ban") => ValidateBanRequest(body),
                    var p when p.Contains("/usermanagement/unban") => ValidateUnbanRequest(body),
                    _ => ValidateGenericInput(body) // Basic validation for all other requests
                };
            }
            catch (JsonException)
            {
                return new ValidationResult 
                { 
                    IsValid = false, 
                    Errors = new[] { "Invalid JSON format" } 
                };
            }
        }

        private static ValidationResult ValidateLoginRequest(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var request = JsonSerializer.Deserialize<AdminLoginRequest>(json, options);
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(request?.Email))
                    errors.Add("Email is required");
                else if (!IsValidEmail(request.Email))
                    errors.Add("Invalid email format");
                else if (request.Email.Length > 255)
                    errors.Add("Email too long (max 255 characters)");

                if (string.IsNullOrWhiteSpace(request?.Password))
                    errors.Add("Password is required");
                else if (request.Password.Length < 6)
                    errors.Add("Password too short (min 6 characters)");
                else if (request.Password.Length > 100)
                    errors.Add("Password too long (max 100 characters)");

                // Check for common injection patterns
                if (ContainsSqlInjectionPatterns(request?.Email) || 
                    ContainsSqlInjectionPatterns(request?.Password))
                    errors.Add("Invalid characters detected");

                return new ValidationResult 
                { 
                    IsValid = errors.Count == 0, 
                    Errors = errors.ToArray() 
                };
            }
            catch
            {
                return new ValidationResult 
                { 
                    IsValid = false, 
                    Errors = new[] { "Invalid request format" } 
                };
            }
        }

        private static ValidationResult ValidateCreateAdminRequest(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var request = JsonSerializer.Deserialize<CreateAdminUserRequest>(json, options);
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(request?.Name))
                    errors.Add("Name is required");
                else if (request.Name.Length > 255)
                    errors.Add("Name too long (max 255 characters)");

                if (string.IsNullOrWhiteSpace(request?.Email))
                    errors.Add("Email is required");
                else if (!IsValidEmail(request.Email))
                    errors.Add("Invalid email format");

                if (string.IsNullOrWhiteSpace(request?.Password))
                    errors.Add("Password is required");
                else if (!IsStrongPassword(request.Password))
                    errors.Add("Password must be at least 8 characters with uppercase, lowercase, number, and special character");

                // Role validation removed - using RoleIds instead

                // Check for injection patterns
                if (ContainsSqlInjectionPatterns(request?.Name) ||
                    ContainsSqlInjectionPatterns(request?.Email))
                    errors.Add("Invalid characters detected");

                return new ValidationResult 
                { 
                    IsValid = errors.Count == 0, 
                    Errors = errors.ToArray() 
                };
            }
            catch
            {
                return new ValidationResult 
                { 
                    IsValid = false, 
                    Errors = new[] { "Invalid request format" } 
                };
            }
        }

        private static ValidationResult ValidateBanRequest(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var request = JsonSerializer.Deserialize<BanUserRequest>(json, options);
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(request?.UserId))
                    errors.Add("UserId is required");
                else if (!IsValidGuid(request.UserId))
                    errors.Add("Invalid UserId format");

                if (string.IsNullOrWhiteSpace(request?.Reason))
                    errors.Add("Ban reason is required");
                else if (request.Reason.Length > 1000)
                    errors.Add("Ban reason too long (max 1000 characters)");

                // BannedBy is handled by the controller from JWT token

                // Validate expiry date if provided
                if (request?.ExpiresAt.HasValue == true && 
                    request.ExpiresAt.Value <= DateTime.UtcNow)
                    errors.Add("Expiry date must be in the future");

                // Check for injection patterns
                if (ContainsSqlInjectionPatterns(request?.Reason))
                    errors.Add("Invalid characters detected");

                return new ValidationResult 
                { 
                    IsValid = errors.Count == 0, 
                    Errors = errors.ToArray() 
                };
            }
            catch
            {
                return new ValidationResult 
                { 
                    IsValid = false, 
                    Errors = new[] { "Invalid request format" } 
                };
            }
        }

        private static ValidationResult ValidateUnbanRequest(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var request = JsonSerializer.Deserialize<UnbanUserRequest>(json, options);
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(request?.UserId))
                    errors.Add("UserId is required");
                else if (!IsValidGuid(request.UserId))
                    errors.Add("Invalid UserId format");

                // UnbannedBy is handled by the controller from JWT token

                if (!string.IsNullOrWhiteSpace(request?.Reason) && 
                    request.Reason.Length > 500)
                    errors.Add("Unban reason too long (max 500 characters)");

                // Check for injection patterns
                if (ContainsSqlInjectionPatterns(request?.Reason))
                    errors.Add("Invalid characters detected");

                return new ValidationResult 
                { 
                    IsValid = errors.Count == 0, 
                    Errors = errors.ToArray() 
                };
            }
            catch
            {
                return new ValidationResult 
                { 
                    IsValid = false, 
                    Errors = new[] { "Invalid request format" } 
                };
            }
        }

        private static ValidationResult ValidateGenericInput(string json)
        {
            var errors = new List<string>();

            // Check for common injection patterns in the entire JSON
            if (ContainsSqlInjectionPatterns(json))
                errors.Add("Potentially malicious input detected");

            // Check for script injection
            if (ContainsScriptInjectionPatterns(json))
                errors.Add("Script injection attempt detected");

            // Check JSON size (prevent DoS)
            if (json.Length > 10000) // 10KB limit
                errors.Add("Request too large");

            return new ValidationResult 
            { 
                IsValid = errors.Count == 0, 
                Errors = errors.ToArray() 
            };
        }

        private static bool IsValidEmail(string email)
        {
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailRegex, RegexOptions.IgnoreCase);
        }

        private static bool IsStrongPassword(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));
        }

        // Role validation removed - now using RoleIds with proper authorization

        private static bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _);
        }

        private static bool ContainsSqlInjectionPatterns(string? input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            var patterns = new[]
            {
                @"('|(\')|(\-\-)|(\;)|(\/\*))",
                @"\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT)\b",
                @"(\<|\>|\""|'|%|script|javascript|vbscript|onload|onerror|onclick)"
            };

            return patterns.Any(pattern => 
                Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }

        private static bool ContainsScriptInjectionPatterns(string input)
        {
            var patterns = new[]
            {
                @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
                @"javascript:",
                @"on\w+\s*=",
                @"<\s*iframe",
                @"<\s*object"
            };

            return patterns.Any(pattern => 
                Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
}