using AdminService.Models.Requests;

namespace AdminService.Validations
{
    public static class UnbanRequestValidator
    {
        public static ValidationResult ValidateUnbanRequest(UnbanUserRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request?.UserId))
                errors.Add("UserId is required");
            else if (!CommonValidationHelpers.IsValidGuid(request.UserId))
                errors.Add("Invalid UserId format");

            if (!string.IsNullOrWhiteSpace(request?.Reason) && 
                request.Reason.Length > 500)
                errors.Add("Unban reason too long (max 500 characters)");

            // Check for injection patterns
            if (CommonValidationHelpers.ContainsSqlInjectionPatterns(request?.Reason))
                errors.Add("Invalid characters detected");

            return new ValidationResult 
            { 
                IsValid = errors.Count == 0, 
                Errors = errors.ToArray() 
            };
        }
    }
}
