using AdminService.Models.Requests;
using System.Text.Json;

namespace AdminService.Validations
{
    public static class BlogManagementRequestValidator
    {
        public static ValidationResult ValidateUpdateBlogPostStatusRequest(UpdateBlogPostStatusRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request?.BlogPostId))
                errors.Add("BlogPostId is required");
            else if (!CommonValidationHelpers.IsValidGuid(request.BlogPostId))
                errors.Add("Invalid BlogPostId format");

            if (request?.Status < 0 || request?.Status > 2)
                errors.Add("Invalid status value. Must be 0 (Draft), 1 (Published), or 2 (Deleted)");

            if (string.IsNullOrWhiteSpace(request?.Reason))
                errors.Add("Reason is required");
            else if (request.Reason.Length < 10)
                errors.Add("Reason must be at least 10 characters");
            else if (request.Reason.Length > 1000)
                errors.Add("Reason too long (max 1000 characters)");

            // Check for injection patterns in reason
            if (CommonValidationHelpers.ContainsObviousSqlInjection(request?.Reason))
                errors.Add("Invalid characters detected in reason");

            return new ValidationResult 
            { 
                IsValid = errors.Count == 0, 
                Errors = errors.ToArray() 
            };
        }

        public static ValidationResult ValidateDeleteBlogPostRequest(DeleteBlogPostRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request?.Reason))
                errors.Add("Reason is required");
            else if (request.Reason.Length < 10)
                errors.Add("Reason must be at least 10 characters");
            else if (request.Reason.Length > 1000)
                errors.Add("Reason too long (max 1000 characters)");

            // Check for injection patterns in reason
            if (CommonValidationHelpers.ContainsObviousSqlInjection(request?.Reason))
                errors.Add("Invalid characters detected in reason");

            return new ValidationResult 
            { 
                IsValid = errors.Count == 0, 
                Errors = errors.ToArray() 
            };
        }
    }
}
