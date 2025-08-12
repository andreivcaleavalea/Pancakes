using AdminService.Models.DTOs;
using AdminService.Models.Entities;
using AdminService.Models.Requests;
using System.Security.Claims;

namespace AdminService.Tests.TestUtilities;

/// <summary>
/// Helper class for creating test data objects.
/// Provides simple, reusable methods to create consistent test data.
/// </summary>
public static class TestDataHelper
{
    // Test Admin User Data
    public static AdminUser CreateTestAdminUser(string email = "test@admin.com", bool isActive = true)
    {
        return new AdminUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            // Use a realistic password hash for consistency with authentication flows
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Name = "Test Admin",
            AdminLevel = 1,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = new List<AdminRole> { CreateTestAdminRole() }
        };
    }

    public static AdminRole CreateTestAdminRole(string name = "SuperAdmin")
    {
        return new AdminRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"{name} role for testing",
            Permissions = GetTestPermissionsJson(),
            Level = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static string GetTestPermissionsJson()
    {
        var permissions = new List<string>
        {
            "CanViewUsers",
            "CanViewUserDetails", 
            "CanBanUsers",
            "CanViewBlogs",
            "CanDeleteBlogs",
            "CanViewDashboard"
        };
        return System.Text.Json.JsonSerializer.Serialize(permissions);
    }

    // Test Request Objects
    public static AdminLoginRequest CreateLoginRequest(string email = "test@admin.com", string password = "password123")
    {
        return new AdminLoginRequest
        {
            Email = email,
            Password = password
        };
    }

    public static CreateAdminUserRequest CreateAdminUserRequest(string email = "newadmin@test.com", int adminLevel = 1)
    {
        return new CreateAdminUserRequest
        {
            Email = email,
            Password = "SecurePassword123!",
            Name = "New Admin",
            AdminLevel = adminLevel,
            RoleIds = new List<Guid> { Guid.NewGuid() }
        };
    }

    public static BlogPostSearchRequest CreateBlogSearchRequest()
    {
        return new BlogPostSearchRequest
        {
            Page = 1,
            PageSize = 10,
            Search = "test",
            Status = 1
        };
    }

    public static UserSearchRequest CreateUserSearchRequest()
    {
        return new UserSearchRequest
        {
            Page = 1,
            PageSize = 10,
            SearchTerm = "test"
        };
    }

    public static UpdateBlogPostStatusRequest CreateUpdateBlogPostStatusRequest(string blogPostId = "12345678-1234-5678-9abc-123456789012", int status = 1)
    {
        return new UpdateBlogPostStatusRequest
        {
            BlogPostId = blogPostId,
            Status = status,
            Reason = "Admin status update - content review completed"
        };
    }

    public static DeleteBlogPostRequest CreateDeleteBlogPostRequest(string blogPostId = "12345678-1234-5678-9abc-123456789012")
    {
        return new DeleteBlogPostRequest
        {
            BlogPostId = blogPostId,
            Reason = "Inappropriate content violating community guidelines",
            DeleteComments = true
        };
    }

    // Test Claims Principal
    public static ClaimsPrincipal CreateTestClaimsPrincipal(string userId = "test-user-id", string email = "test@admin.com")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new("permission", "CanViewUsers"),
            new("permission", "CanViewUserDetails"),
            new("permission", "CanBanUsers"),
            new("permission", "CanViewBlogs"),
            new("permission", "CanDeleteBlogs"),
            new("permission", "CanManageBlogs"),
            new("permission", "CanViewAnalytics"),
            new("permission", "CanViewDashboard")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}