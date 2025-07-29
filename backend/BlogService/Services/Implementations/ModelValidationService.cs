using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogService.Services.Implementations;

public class ModelValidationService : IModelValidationService
{
    private readonly ILogger<ModelValidationService> _logger;

    public ModelValidationService(ILogger<ModelValidationService> logger)
    {
        _logger = logger;
    }

    public ValidationResult ValidateModel(ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
        {
            return new ValidationResult { IsValid = true };
        }

        var errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
            .ToArray();

        var errorMessage = string.Join("; ", errors);
        _logger.LogWarning("Model state invalid: {Errors}", errorMessage);

        return new ValidationResult 
        { 
            IsValid = false, 
            ErrorMessage = errorMessage,
            ErrorDetails = modelState
        };
    }
}
