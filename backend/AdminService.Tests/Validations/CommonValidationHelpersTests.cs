using AdminService.Validations;
using FluentAssertions;
using Xunit;

namespace AdminService.Tests.Validations;

public class CommonValidationHelpersTests
{
    [Fact]
    public void CommonValidationHelpers_ValidateEmail()
    {
        // Test valid emails
        CommonValidationHelpers.IsValidEmail("test@example.com").Should().BeTrue();
        CommonValidationHelpers.IsValidEmail("user.name@domain.co.uk").Should().BeTrue();
        CommonValidationHelpers.IsValidEmail("admin+tag@company.org").Should().BeTrue();

        // Test invalid emails
        CommonValidationHelpers.IsValidEmail("").Should().BeFalse();
        CommonValidationHelpers.IsValidEmail("invalid-email").Should().BeFalse();
        CommonValidationHelpers.IsValidEmail("@domain.com").Should().BeFalse();
        CommonValidationHelpers.IsValidEmail("user@").Should().BeFalse();
        CommonValidationHelpers.IsValidEmail("user.name@domain.com").Should().BeTrue();
    }

    [Fact]
    public void CommonValidationHelpers_ValidateStrongPassword()
    {
        // Test valid passwords
        CommonValidationHelpers.IsStrongPassword("SecurePass123!").Should().BeTrue();
        CommonValidationHelpers.IsStrongPassword("MyPassword1@").Should().BeTrue();
        CommonValidationHelpers.IsStrongPassword("AdminUser2024#").Should().BeTrue();

        // Test invalid passwords
        CommonValidationHelpers.IsStrongPassword("").Should().BeFalse();
        CommonValidationHelpers.IsStrongPassword("short").Should().BeFalse();
        CommonValidationHelpers.IsStrongPassword("alllowercase123!").Should().BeFalse();
        CommonValidationHelpers.IsStrongPassword("ALLUPPERCASE123!").Should().BeFalse();
        CommonValidationHelpers.IsStrongPassword("NoNumbers!").Should().BeFalse();
        CommonValidationHelpers.IsStrongPassword("OnlyLetters123").Should().BeFalse();
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012", true)]
    [InlineData("invalid-guid", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void CommonValidationHelpers_ValidateGuid(string input, bool expected)
    {
        CommonValidationHelpers.IsValidGuid(input).Should().Be(expected);
    }
}