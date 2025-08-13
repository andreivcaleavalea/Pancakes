using AdminService.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace AdminService.Tests.Authorization;

public class AuthorizationPolicyBuilderExtensionsTests
{
    [Fact]
    public void RequirePermission_WhenCalled_ShouldAddPermissionRequirement()
    {
        const string permission = "users:view";
        var policyBuilder = new AuthorizationPolicyBuilder();

        var result = policyBuilder.RequirePermission(permission);

        result.Should().BeSameAs(policyBuilder, "Extension method should return the same builder for chaining");
        
        var policy = policyBuilder.Build();
        policy.Requirements.Should().HaveCount(1);
        policy.Requirements.First().Should().BeOfType<PermissionRequirement>();
        
        var permissionRequirement = policy.Requirements.First() as PermissionRequirement;
        permissionRequirement!.Permission.Should().Be(permission);
    }

    [Fact]
    public void RequirePermission_WhenCalledMultipleTimes_ShouldAddMultipleRequirements()
    {
        var policyBuilder = new AuthorizationPolicyBuilder();
        const string permission1 = "users:view";
        const string permission2 = "users:ban";
        const string permission3 = "blogs:manage";

        policyBuilder
            .RequirePermission(permission1)
            .RequirePermission(permission2)
            .RequirePermission(permission3);

        var policy = policyBuilder.Build();
        policy.Requirements.Should().HaveCount(3);
        
        var permissionRequirements = policy.Requirements.OfType<PermissionRequirement>().ToList();
        permissionRequirements.Should().HaveCount(3);
        
        permissionRequirements[0].Permission.Should().Be(permission1);
        permissionRequirements[1].Permission.Should().Be(permission2);
        permissionRequirements[2].Permission.Should().Be(permission3);
    }

    [Fact]
    public void RequirePermission_WhenCalledWithAdminPermissionConstants_ShouldWork()
    {
        var policyBuilder = new AuthorizationPolicyBuilder();

        policyBuilder
            .RequirePermission(AdminPermissions.ViewUsers)
            .RequirePermission(AdminPermissions.BanUsers)
            .RequirePermission(AdminPermissions.ViewBlogs);

        var policy = policyBuilder.Build();
        var permissionRequirements = policy.Requirements.OfType<PermissionRequirement>().ToList();
        
        permissionRequirements.Should().HaveCount(3);
        permissionRequirements[0].Permission.Should().Be(AdminPermissions.ViewUsers);
        permissionRequirements[1].Permission.Should().Be(AdminPermissions.BanUsers);
        permissionRequirements[2].Permission.Should().Be(AdminPermissions.ViewBlogs);
    }

    [Theory]
    [InlineData("users:view")]
    [InlineData("blogs:manage")]
    [InlineData("analytics:dashboard")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UPPERCASE:PERMISSION")]
    [InlineData("special-chars:permission!@#$")]
    public void RequirePermission_WhenCalledWithVariousPermissions_ShouldStoreExactValue(string permission)
    {
        var policyBuilder = new AuthorizationPolicyBuilder();

        policyBuilder.RequirePermission(permission);

        var policy = policyBuilder.Build();
        var permissionRequirement = policy.Requirements.OfType<PermissionRequirement>().First();
        permissionRequirement.Permission.Should().Be(permission);
    }

    [Fact]
    public void RequirePermission_WhenCalledWithNullPermission_ShouldThrowArgumentNullException()
    {
        var policyBuilder = new AuthorizationPolicyBuilder();

        var action = () => policyBuilder.RequirePermission(null!);
        
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("permission");
    }

    [Fact]
    public void RequirePermission_WhenChainedWithOtherPolicyMethods_ShouldWork()
    {
        var policyBuilder = new AuthorizationPolicyBuilder();

        policyBuilder
            .RequireAuthenticatedUser()
            .RequirePermission(AdminPermissions.ViewUsers)
            .RequireClaim("role", "admin");

        var policy = policyBuilder.Build();
        policy.Requirements.Should().HaveCount(3);
        
        policy.Requirements.Should().Contain(r => r.GetType().Name.Contains("DenyAnonymous"));
        
        var permissionRequirement = policy.Requirements.OfType<PermissionRequirement>().FirstOrDefault();
        permissionRequirement.Should().NotBeNull();
        permissionRequirement!.Permission.Should().Be(AdminPermissions.ViewUsers);
        
        policy.Requirements.Should().Contain(r => r.GetType().Name.Contains("Claims"));
    }

    [Fact]
    public void RequirePermission_WhenCalledOnNullBuilder_ShouldThrowArgumentNullException()
    {
        AuthorizationPolicyBuilder? nullBuilder = null;

        var action = () => nullBuilder!.RequirePermission("test:permission");
        
        action.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void RequirePermission_ShouldAllowMethodChaining()
    {
        var policyBuilder = new AuthorizationPolicyBuilder();

        var result = policyBuilder
            .RequirePermission("first:permission")
            .RequirePermission("second:permission")
            .RequirePermission("third:permission");

        result.Should().BeSameAs(policyBuilder);
    }

    [Fact]
    public void RequirePermission_WhenUsedInCompleteAuthorizationScenario_ShouldCreateValidPolicy()
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequirePermission(AdminPermissions.ViewUsers)
            .RequirePermission(AdminPermissions.BanUsers)
            .Build();

        policy.Should().NotBeNull();
        policy.Requirements.Should().HaveCount(3);
        
        var permissionRequirements = policy.Requirements.OfType<PermissionRequirement>().ToList();
        permissionRequirements.Should().HaveCount(2);
        
        permissionRequirements.Should().Contain(r => r.Permission == AdminPermissions.ViewUsers);
        permissionRequirements.Should().Contain(r => r.Permission == AdminPermissions.BanUsers);
    }
}
