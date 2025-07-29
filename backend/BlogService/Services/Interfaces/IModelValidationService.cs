using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogService.Services.Interfaces;

public interface IModelValidationService
{
    ValidationResult ValidateModel(ModelStateDictionary modelState);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public object? ErrorDetails { get; set; }
}
