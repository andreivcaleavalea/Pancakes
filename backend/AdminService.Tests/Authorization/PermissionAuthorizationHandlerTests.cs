using AdminService.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AdminService.Tests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        var mockLogger = new Mock<ILogger<PermissionAuthorizationHandler>>();
        _handler = new PermissionAuthorizationHandler(mockLogger.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasPermission_SucceedsAuthorization()
    {
        var requirement = new PermissionRequirement("CanViewUsers");
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(ClaimTypes.Email, "test@admin.com"),
            new("permission", "CanViewUsers")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserDoesNotHavePermission_FailsAuthorization()
    {
        var requirement = new PermissionRequirement("CanDeleteBlogs");
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(ClaimTypes.Email, "test@admin.com"),
            new("permission", "CanViewUsers") // Different permission
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsNotAuthenticated_FailsAuthorization()
    {
        var requirement = new PermissionRequirement("CanViewUsers");
        var user = new ClaimsPrincipal(); // No authenticated identity
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasMultiplePermissions_SucceedsForCorrectPermission()
    {
        var requirement = new PermissionRequirement("CanBanUsers");
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(ClaimTypes.Email, "test@admin.com"),
            new("permission", "CanViewUsers"),
            new("permission", "CanBanUsers"),
            new("permission", "CanViewBlogs")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasNoPermissionClaims_FailsAuthorization()
    {
        var requirement = new PermissionRequirement("CanViewUsers");
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(ClaimTypes.Email, "test@admin.com")
            // No permission claims
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasEmptyIdentity_FailsAuthorization()
    {
        var requirement = new PermissionRequirement("CanViewUsers");
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Empty identity (not authenticated)
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("CanViewUsers")]
    [InlineData("CanViewUserDetails")]
    [InlineData("CanBanUsers")]
    [InlineData("CanViewBlogs")]
    [InlineData("CanDeleteBlogs")]
    [InlineData("CanViewAnalytics")]
    public async Task HandleRequirementAsync_WithVariousPermissions_WorksCorrectly(string permission)
    {
        var requirement = new PermissionRequirement(permission);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(ClaimTypes.Email, "test@admin.com"),
            new("permission", permission)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }
}