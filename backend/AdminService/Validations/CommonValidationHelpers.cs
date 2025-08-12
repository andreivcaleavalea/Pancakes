using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AdminService.Validations
{
    public static class CommonValidationHelpers
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var addr = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsStrongPassword(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));
        }

        public static bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _);
        }

        public static bool ContainsSqlInjectionPatterns(string? input)
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

        public static bool ContainsObviousSqlInjection(string? input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            // More lenient patterns - only flag obvious SQL injection attempts
            var patterns = new[]
            {
                @"(';|';\s*--|';\s*(DROP|DELETE|INSERT|UPDATE|SELECT))",
                @"\b(DROP\s+TABLE|DELETE\s+FROM|INSERT\s+INTO)\b",
                @"(UNION\s+SELECT|--\s*$)",
                @"(\/\*.*\*\/)",
                @"(\bOR\b.*=.*\bOR\b)",
                @"(1=1|'=')"
            };

            return patterns.Any(pattern => 
                Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }

        public static bool ContainsScriptInjectionPatterns(string input)
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
}
