using AdminService.Models.Requests;

namespace AdminService.Validations
{
    public static class LoginRequestValidator
    {
        public static ValidationResult ValidateLoginRequest(AdminLoginRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request?.Email))
                errors.Add("Email is required");
            else if (!CommonValidationHelpers.IsValidEmail(request.Email))
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
            if (CommonValidationHelpers.ContainsSqlInjectionPatterns(request?.Email) || 
                CommonValidationHelpers.ContainsSqlInjectionPatterns(request?.Password))
                errors.Add("Invalid characters detected");

            return new ValidationResult 
            { 
                IsValid = errors.Count == 0, 
                Errors = errors.ToArray() 
            };
        }
    }
}
