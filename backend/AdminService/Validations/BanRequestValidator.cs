using AdminService.Models.Requests;

namespace AdminService.Validations
{
    public static class BanRequestValidator
    {
        public static ValidationResult ValidateBanRequest(BanUserRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request?.UserId))
                errors.Add("UserId is required");
            else if (!CommonValidationHelpers.IsValidGuid(request.UserId))
                errors.Add("Invalid UserId format");

            if (string.IsNullOrWhiteSpace(request?.Reason))
                errors.Add("Ban reason is required");
            else if (request.Reason.Length > 1000)
                errors.Add("Ban reason too long (max 1000 characters)");

            // Validate expiry date if provided
            if (request?.ExpiresAt.HasValue == true && 
                request.ExpiresAt.Value <= DateTime.UtcNow)
                errors.Add("Expiry date must be in the future");

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
