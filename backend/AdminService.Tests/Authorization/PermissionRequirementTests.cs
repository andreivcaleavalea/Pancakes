using AdminService.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace AdminService.Tests.Authorization;

/// <summary>
/// Tests for PermissionRequirement class.
/// Ensures the authorization requirement properly stores and validates permissions.
/// </summary>
public class PermissionRequirementTests
{
    [Fact]
    public void PermissionRequirement_WhenCreatedWithValidPermission_ShouldStorePermission()
    {
        // Arrange
        const string expectedPermission = "users:view";

        // Act
        var requirement = new PermissionRequirement(expectedPermission);

        // Assert
        requirement.Permission.Should().Be(expectedPermission);
        requirement.Should().BeAssignableTo<IAuthorizationRequirement>();
    }

    [Fact]
    public void PermissionRequirement_WhenCreatedWithNullPermission_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var action = () => new PermissionRequirement(null!);
        
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("permission");
    }

    [Fact]
    public void PermissionRequirement_WhenCreatedWithEmptyPermission_ShouldStoreEmptyString()
    {
        // Arrange
        const string emptyPermission = "";

        // Act
        var requirement = new PermissionRequirement(emptyPermission);

        // Assert
        requirement.Permission.Should().Be(emptyPermission);
    }

    [Fact]
    public void PermissionRequirement_WhenCreatedWithWhitespacePermission_ShouldStoreWhitespace()
    {
        // Arrange
        const string whitespacePermission = "   ";

        // Act
        var requirement = new PermissionRequirement(whitespacePermission);

        // Assert
        requirement.Permission.Should().Be(whitespacePermission);
    }

    [Theory]
    [InlineData("users:view")]
    [InlineData("blogs:manage")]
    [InlineData("analytics:dashboard")]
    [InlineData("audit:view")]
    [InlineData("UPPERCASE:PERMISSION")]
    [InlineData("special-chars:permission!@#$")]
    [InlineData("long:permission:with:multiple:colons")]
    public void PermissionRequirement_WhenCreatedWithVariousPermissions_ShouldStoreExactValue(string permission)
    {
        // Act
        var requirement = new PermissionRequirement(permission);

        // Assert
        requirement.Permission.Should().Be(permission);
    }

    [Fact]
    public void PermissionRequirement_ShouldImplementIAuthorizationRequirement()
    {
        // Arrange
        var requirement = new PermissionRequirement("test:permission");

        // Act & Assert
        requirement.Should().BeAssignableTo<IAuthorizationRequirement>();
    }

    [Fact]
    public void PermissionRequirement_WhenCreatedWithAdminPermissionConstants_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var requirements = new[]
        {
            new PermissionRequirement(AdminPermissions.ViewUsers),
            new PermissionRequirement(AdminPermissions.BanUsers),
            new PermissionRequirement(AdminPermissions.ViewBlogs),
            new PermissionRequirement(AdminPermissions.ViewAnalytics),
            new PermissionRequirement(AdminPermissions.ViewAuditLogs)
        };

        // Assert
        requirements[0].Permission.Should().Be(AdminPermissions.ViewUsers);
        requirements[1].Permission.Should().Be(AdminPermissions.BanUsers);
        requirements[2].Permission.Should().Be(AdminPermissions.ViewBlogs);
        requirements[3].Permission.Should().Be(AdminPermissions.ViewAnalytics);
        requirements[4].Permission.Should().Be(AdminPermissions.ViewAuditLogs);

        requirements.Should().AllSatisfy(r => r.Should().BeAssignableTo<IAuthorizationRequirement>());
    }

    [Fact]
    public void PermissionRequirement_Permission_ShouldBeReadOnly()
    {
        // Arrange
        var requirement = new PermissionRequirement("test:permission");

        // Act & Assert
        var propertyInfo = typeof(PermissionRequirement).GetProperty(nameof(PermissionRequirement.Permission));
        propertyInfo.Should().NotBeNull();
        propertyInfo!.CanRead.Should().BeTrue();
        propertyInfo.CanWrite.Should().BeFalse("Permission property should be read-only");
    }

    [Fact]
    public void PermissionRequirement_MultipleInstances_WithSamePermission_ShouldBeEqual()
    {
        // Arrange
        const string permission = "users:view";
        var requirement1 = new PermissionRequirement(permission);
        var requirement2 = new PermissionRequirement(permission);

        // Act & Assert
        requirement1.Permission.Should().Be(requirement2.Permission);
    }

    [Fact]
    public void PermissionRequirement_MultipleInstances_WithDifferentPermissions_ShouldNotBeEqual()
    {
        // Arrange
        var requirement1 = new PermissionRequirement("users:view");
        var requirement2 = new PermissionRequirement("users:ban");

        // Act & Assert
        requirement1.Permission.Should().NotBe(requirement2.Permission);
    }
}
