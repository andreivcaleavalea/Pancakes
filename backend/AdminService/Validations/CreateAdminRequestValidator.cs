using AdminService.Models.Requests;

namespace AdminService.Validations
{
    public static class CreateAdminRequestValidator
    {
        public static ValidationResult ValidateCreateAdminRequest(CreateAdminUserRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request?.Name))
                errors.Add("Name is required");
            else if (request.Name.Length > 255)
                errors.Add("Name too long (max 255 characters)");

            if (string.IsNullOrWhiteSpace(request?.Email))
                errors.Add("Email is required");
            else if (!CommonValidationHelpers.IsValidEmail(request.Email))
                errors.Add("Invalid email format");

            if (string.IsNullOrWhiteSpace(request?.Password))
                errors.Add("Password is required");
            else if (!CommonValidationHelpers.IsStrongPassword(request.Password))
                errors.Add("Password must be at least 8 characters with uppercase, lowercase, number, and special character");

            // Check for injection patterns
            if (CommonValidationHelpers.ContainsSqlInjectionPatterns(request?.Name) ||
                CommonValidationHelpers.ContainsSqlInjectionPatterns(request?.Email))
                errors.Add("Invalid characters detected");

            return new ValidationResult 
            { 
                IsValid = errors.Count == 0, 
                Errors = errors.ToArray() 
            };
        }
    }
}
