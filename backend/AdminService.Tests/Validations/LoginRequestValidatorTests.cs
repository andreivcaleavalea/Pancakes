using AdminService.Models.Requests;
using AdminService.Validations;
using FluentAssertions;
using Xunit;

namespace AdminService.Tests.Validations;

public class LoginRequestValidatorTests
{
    [Fact]
    public void ValidateLoginRequest_WhenValid()
    {
        var request = new AdminLoginRequest
        {
            Email = "admin@example.com",
            Password = "SecurePassword123!",
            TwoFactorCode = "123456"
        };

        var result = LoginRequestValidator.ValidateLoginRequest(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateLoginRequest_WhenEmptyEmail(string email)
    {
        var request = new AdminLoginRequest
        {
            Email = email,
            Password = "SecurePassword123!"
        };

        var result = LoginRequestValidator.ValidateLoginRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user.domain.com")]
    public void ValidateLoginRequest_WhenInvalidEmailFormat(string email)
    {
        var request = new AdminLoginRequest
        {
            Email = email,
            Password = "SecurePassword123!"
        };

        var result = LoginRequestValidator.ValidateLoginRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email format");
    }

    [Fact]
    public void ValidateLoginRequest_WhenEmailTooLong()
    {
        var longEmail = new string('a', 250) + "@example.com"; // > 255 characters
        var request = new AdminLoginRequest
        {
            Email = longEmail,
            Password = "SecurePassword123!"
        };

        var result = LoginRequestValidator.ValidateLoginRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Email too long (max 255 characters)");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateLoginRequest_WhenEmptyPassword(string password)
    {
        var request = new AdminLoginRequest
        {
            Email = "admin@example.com",
            Password = password
        };

        var result = LoginRequestValidator.ValidateLoginRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password is required");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("12345")]
    public void ValidateLoginRequest_WhenPasswordTooShort(string password)
    {
        var request = new AdminLoginRequest
        {
            Email = "admin@example.com",
            Password = password
        };

        var result = LoginRequestValidator.ValidateLoginRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password too short (min 6 characters)");
    }

    [Fact]
    public void ValidateLoginRequest_WhenPasswordTooLong()
    {
        var longPassword = new string('a', 101); // > 100 characters
        var request = new AdminLoginRequest
        {
            Email = "admin@example.com",
            Password = longPassword
        };

        var result = LoginRequestValidator.ValidateLoginRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password too long (max 100 characters)");
    }

    [Theory]
    [InlineData("admin@example.com'; DROP TABLE users; --")]
    [InlineData("password'; DELETE FROM admins; --")]
    public void ValidateLoginRequest_WhenContainsSqlInjection(string maliciousInput)
    {
        var request = new AdminLoginRequest
        {
            Email = maliciousInput,
            Password = maliciousInput
        };

        var result = LoginRequestValidator.ValidateLoginRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Invalid characters detected");
    }

    [Fact]
    public void ValidateLoginRequest_WhenNullRequest()
    {
        var result = LoginRequestValidator.ValidateLoginRequest(null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Email is required");
        result.Errors.Should().Contain("Password is required");
    }
}
