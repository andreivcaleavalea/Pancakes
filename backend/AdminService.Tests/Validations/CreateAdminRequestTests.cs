using AdminService.Models.Requests;
using AdminService.Validations;
using FluentAssertions;
using Xunit;

namespace AdminService.Tests.Validations;

public class CreateAdminRequestTests
{
    [Fact]
    public void ValidateCreateAdminRequest_WhenValid()
    {
        var request = new CreateAdminUserRequest
        {
            Name = "John Doe",
            Email = "test@admin.com",
            Password = "SecurePassword123!",
            AdminLevel = 1,
            RoleIds = new List<Guid> { Guid.NewGuid() }
        };

        var result = CreateAdminRequestValidator.ValidateCreateAdminRequest(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@domain.com")]
    public void ValidateCreateAdminRequest_WhenInvalidEmail(string email)
    {
        var request = new CreateAdminUserRequest
        {
            Name = "John Doe",
            Email = email,
            Password = "SecurePassword123!",
            AdminLevel = 1,
            RoleIds = new List<Guid> { Guid.NewGuid() }
        };

        var result = CreateAdminRequestValidator.ValidateCreateAdminRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("email", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("password")]
    [InlineData("PASSWORD")]
    [InlineData("Password")]
    public void ValidateCreateAdminRequest_WhenInvalidPassword(string password)
    {
        var request = new CreateAdminUserRequest
        {
            Name = "John Doe",
            Email = "test@admin.com",
            Password = password,
            AdminLevel = 1,
            RoleIds = new List<Guid> { Guid.NewGuid() }
        };

        var result = CreateAdminRequestValidator.ValidateCreateAdminRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateCreateAdminRequest_WhenEmptyName(string name)
    {
        var request = new CreateAdminUserRequest
        {
            Name = name,
            Email = "test@admin.com",
            Password = "SecurePassword123!",
            AdminLevel = 1,
            RoleIds = new List<Guid> { Guid.NewGuid() }
        };

        var result = CreateAdminRequestValidator.ValidateCreateAdminRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Name", StringComparison.OrdinalIgnoreCase));
    }
}