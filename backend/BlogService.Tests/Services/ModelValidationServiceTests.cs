using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class ModelValidationServiceTests
{
    private static ModelValidationService CreateService()
    {
        var logger = new Mock<ILogger<ModelValidationService>>();
        return new ModelValidationService(logger.Object);
    }

    private static ModelStateDictionary CreateValidModelState()
    {
        return new ModelStateDictionary();
    }

    private static ModelStateDictionary CreateInvalidModelState()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Property1", "Error message 1");
        modelState.AddModelError("Property2", "Error message 2a");
        modelState.AddModelError("Property2", "Error message 2b");
        return modelState;
    }

    [Fact]
    public void ValidateModel_ValidModelState_ReturnsValid()
    {
        var service = CreateService();
        var modelState = CreateValidModelState();

        var result = service.ValidateModel(modelState);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void ValidateModel_InvalidModelState_ReturnsInvalid()
    {
        var service = CreateService();
        var modelState = CreateInvalidModelState();

        var result = service.ValidateModel(modelState);

        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorDetails.Should().BeSameAs(modelState);
    }

    [Fact]
    public void ValidateModel_InvalidModelState_FormatsErrorsCorrectly()
    {
        var service = CreateService();
        var modelState = CreateInvalidModelState();

        var result = service.ValidateModel(modelState);

        result.ErrorMessage.Should().Contain("Property1: Error message 1");
        result.ErrorMessage.Should().Contain("Property2: Error message 2a, Error message 2b");
        result.ErrorMessage.Should().Contain(";");
    }

    [Fact]
    public void ValidateModel_SingleError_FormatsCorrectly()
    {
        var service = CreateService();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("TestProperty", "Single error message");

        var result = service.ValidateModel(modelState);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("TestProperty: Single error message");
    }

    [Fact]
    public void ValidateModel_MultipleErrorsOnSameProperty_CombinesMessages()
    {
        var service = CreateService();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Email", "Email is required");
        modelState.AddModelError("Email", "Email format is invalid");

        var result = service.ValidateModel(modelState);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Email: Email is required, Email format is invalid");
    }

    [Fact]
    public void ValidateModel_EmptyErrorMessage_HandlesGracefully()
    {
        var service = CreateService();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Property", string.Empty);

        var result = service.ValidateModel(modelState);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Property: ");
    }

    [Fact]
    public void ValidateModel_PropertyWithOnlyValidEntries_DoesNotIncludeInError()
    {
        var service = CreateService();
        var modelState = new ModelStateDictionary();
        
        // Add a valid entry (no errors)
        modelState.SetModelValue("ValidProperty", new ValueProviderResult("valid value"));
        
        // Add an invalid entry
        modelState.AddModelError("InvalidProperty", "Error message");

        var result = service.ValidateModel(modelState);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("InvalidProperty: Error message");
        result.ErrorMessage.Should().NotContain("ValidProperty");
    }

    [Fact]
    public void ValidateModel_ComplexErrorStructure_FormatsAllErrors()
    {
        var service = CreateService();
        var modelState = new ModelStateDictionary();
        
        modelState.AddModelError("User.FirstName", "First name is required");
        modelState.AddModelError("User.LastName", "Last name is required");
        modelState.AddModelError("User.Email", "Email is required");
        modelState.AddModelError("User.Email", "Email must be valid");
        modelState.AddModelError("Address.Street", "Street is required");

        var result = service.ValidateModel(modelState);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("User.FirstName: First name is required");
        result.ErrorMessage.Should().Contain("User.LastName: Last name is required");
        result.ErrorMessage.Should().Contain("User.Email: Email is required, Email must be valid");
        result.ErrorMessage.Should().Contain("Address.Street: Street is required");
        
        // Should have semicolons separating different properties
        var semicolonCount = result.ErrorMessage.Count(c => c == ';');
        semicolonCount.Should().Be(3); // 4 properties - 1 = 3 semicolons
    }

    [Fact]
    public void ValidateModel_NullModelState_ThrowsException()
    {
        var service = CreateService();

        Action act = () => service.ValidateModel(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
