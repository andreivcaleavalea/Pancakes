using AdminService.Authorization;
using FluentAssertions;
using Xunit;

namespace AdminService.Tests.Authorization;

public class AdminPermissionsTests
{
    [Fact]
    public void AdminPermissions_ShouldHaveUserManagementPermissions()
    {
        AdminPermissions.ViewUsers.Should().Be("users:view");
        AdminPermissions.BanUsers.Should().Be("users:ban");
        AdminPermissions.UnbanUsers.Should().Be("users:unban");
        AdminPermissions.ViewUserDetails.Should().Be("users:details");
        AdminPermissions.UpdateUsers.Should().Be("users:update");
    }

    [Fact]
    public void AdminPermissions_ShouldHaveBlogManagementPermissions()
    {
        AdminPermissions.ViewBlogs.Should().Be("blogs:view");
        AdminPermissions.ViewBlogDetails.Should().Be("blogs:details");
        AdminPermissions.ManageBlogs.Should().Be("blogs:manage");
        AdminPermissions.DeleteBlogs.Should().Be("blogs:delete");
    }

    [Fact]
    public void AdminPermissions_ShouldHaveAnalyticsPermissions()
    {
        AdminPermissions.ViewAnalytics.Should().Be("analytics:view");
        AdminPermissions.ViewDashboard.Should().Be("analytics:dashboard");
    }

    [Fact]
    public void AdminPermissions_ShouldHaveAuditPermissions()
    {
        AdminPermissions.ViewAuditLogs.Should().Be("audit:view");
    }

    [Fact]
    public void AdminPermissions_ShouldFollowConsistentNamingPattern()
    {
        var permissionFields = typeof(AdminPermissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .ToList();

        permissionFields.Should().NotBeEmpty();
        
        foreach (var field in permissionFields)
        {
            var value = (string)field.GetValue(null)!;
            
            value.Should().Contain(":", $"Permission {field.Name} should follow 'resource:action' pattern");
            
            value.Should().NotBeNullOrWhiteSpace($"Permission {field.Name} should have a value");
            
            value.Should().Be(value.ToLowerInvariant(), $"Permission {field.Name} should be lowercase");
        }
    }

    [Fact]
    public void AdminPermissions_ShouldHaveUniqueValues()
    {
        var permissionValues = typeof(AdminPermissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToList();

        permissionValues.Should().OnlyHaveUniqueItems("All permissions should have unique values");
    }

    [Theory]
    [InlineData("users")]
    [InlineData("blogs")]
    [InlineData("analytics")]
    [InlineData("audit")]
    public void AdminPermissions_ShouldHavePermissionsForResource(string resource)
    {
        var permissionValues = typeof(AdminPermissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToList();

        permissionValues.Should().Contain(p => p.StartsWith($"{resource}:"), 
            $"Should have at least one permission for resource '{resource}'");
    }

    [Fact]
    public void AdminPermissions_ConstantsShouldBeAccessible()
    {
        var userPermissions = new[]
        {
            AdminPermissions.ViewUsers,
            AdminPermissions.BanUsers,
            AdminPermissions.UnbanUsers,
            AdminPermissions.ViewUserDetails,
            AdminPermissions.UpdateUsers
        };

        var blogPermissions = new[]
        {
            AdminPermissions.ViewBlogs,
            AdminPermissions.ViewBlogDetails,
            AdminPermissions.ManageBlogs,
            AdminPermissions.DeleteBlogs
        };

        var analyticsPermissions = new[]
        {
            AdminPermissions.ViewAnalytics,
            AdminPermissions.ViewDashboard
        };

        var auditPermissions = new[]
        {
            AdminPermissions.ViewAuditLogs
        };

        userPermissions.Should().AllSatisfy(p => p.Should().NotBeNullOrWhiteSpace());
        blogPermissions.Should().AllSatisfy(p => p.Should().NotBeNullOrWhiteSpace());
        analyticsPermissions.Should().AllSatisfy(p => p.Should().NotBeNullOrWhiteSpace());
        auditPermissions.Should().AllSatisfy(p => p.Should().NotBeNullOrWhiteSpace());
    }
}
