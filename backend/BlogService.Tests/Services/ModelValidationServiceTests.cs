using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;

namespace BlogService.Tests.Services;

public class ModelValidationServiceTests
{
    private static ModelValidationService CreateService(out Mock<ILogger<ModelValidationService>> loggerMock)
    {
        loggerMock = new Mock<ILogger<ModelValidationService>>();
        return new ModelValidationService(loggerMock.Object);
    }

    [Fact]
    public void ValidateModel_WithValidModelState_ReturnsValidResult()
    {
        // Arrange
        var service = CreateService(out var loggerMock);
        var modelState = new ModelStateDictionary();
        // ModelState is valid by default when no errors are added

        // Act
        var result = service.ValidateModel(modelState);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        result.ErrorDetails.Should().BeNull();
        
        // Verify no logging occurred for valid state
        loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void ValidateModel_WithSingleError_ReturnsInvalidResultWithError()
    {
        // Arrange
        var service = CreateService(out var loggerMock);
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Property1", "Property1 is required");

        // Act
        var result = service.ValidateModel(modelState);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Property1: Property1 is required");
        result.ErrorDetails.Should().Be(modelState);
        
        // Verify logging occurred
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Model state invalid")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ValidateModel_WithMultipleErrors_ReturnsInvalidResultWithAllErrors()
    {
        // Arrange
        var service = CreateService(out var loggerMock);
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Property1", "Property1 is required");
        modelState.AddModelError("Property2", "Property2 must be at least 5 characters");
        modelState.AddModelError("Property3", "Property3 is invalid");

        // Act
        var result = service.ValidateModel(modelState);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Property1: Property1 is required");
        result.ErrorMessage.Should().Contain("Property2: Property2 must be at least 5 characters");
        result.ErrorMessage.Should().Contain("Property3: Property3 is invalid");
        result.ErrorMessage.Split(';').Should().HaveCount(3);
        result.ErrorDetails.Should().Be(modelState);
    }

    [Fact]
    public void ValidateModel_WithMultipleErrorsForSameProperty_CombinesErrors()
    {
        // Arrange
        var service = CreateService(out var loggerMock);
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Email", "Email is required");
        modelState.AddModelError("Email", "Email format is invalid");

        // Act
        var result = service.ValidateModel(modelState);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Email: Email is required, Email format is invalid");
        result.ErrorDetails.Should().Be(modelState);
    }

    [Fact]
    public void ValidateModel_WithEmptyErrorMessage_HandlesGracefully()
    {
        // Arrange
        var service = CreateService(out var loggerMock);
        var modelState = new ModelStateDictionary();
        
        // Add an error with empty message (edge case)
        var modelError = new ModelError(string.Empty);
        modelState.SetModelValue("Property1", new ValueProviderResult("value"));
        modelState["Property1"]!.Errors.Add(modelError);

        // Act
        var result = service.ValidateModel(modelState);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Property1: ");
        result.ErrorDetails.Should().Be(modelState);
    }

    [Fact]
    public void ValidateModel_WithSpecialCharactersInPropertyName_HandlesCorrectly()
    {
        // Arrange
        var service = CreateService(out var loggerMock);
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("User.Profile.Name", "Name is required");
        modelState.AddModelError("Items[0].Title", "Title cannot be empty");

        // Act
        var result = service.ValidateModel(modelState);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("User.Profile.Name: Name is required");
        result.ErrorMessage.Should().Contain("Items[0].Title: Title cannot be empty");
        result.ErrorDetails.Should().Be(modelState);
    }

    [Fact]
    public void ValidateModel_WithNullModelState_ThrowsNullReferenceException()
    {
        // Arrange
        var service = CreateService(out var loggerMock);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => service.ValidateModel(null!));
    }

    [Fact]
    public void ValidateModel_LogsWarningWithCorrectMessage()
    {
        // Arrange
        var service = CreateService(out var loggerMock);
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("TestProperty", "Test error message");

        // Act
        service.ValidateModel(modelState);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) => 
                    state.ToString()!.Contains("Model state invalid") && 
                    state.ToString()!.Contains("TestProperty: Test error message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ValidateModel_WithMixedValidAndInvalidProperties_OnlyReturnsErrors()
    {
        // Arrange
        var service = CreateService(out var loggerMock);
        var modelState = new ModelStateDictionary();
        
        // Add valid property (no errors)
        modelState.SetModelValue("ValidProperty", new ValueProviderResult("valid value"));
        
        // Add invalid property (with error)
        modelState.AddModelError("InvalidProperty", "This property has an error");

        // Act
        var result = service.ValidateModel(modelState);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("InvalidProperty: This property has an error");
        result.ErrorMessage.Should().NotContain("ValidProperty");
        result.ErrorDetails.Should().Be(modelState);
    }
}
