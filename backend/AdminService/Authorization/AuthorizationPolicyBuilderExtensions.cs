using Microsoft.AspNetCore.Authorization;

namespace AdminService.Authorization
{
    /// Extension methods for easier authorization policy configuration
    public static class AuthorizationPolicyBuilderExtensions
    {
        /// Adds a permission requirement to the authorization policy
        public static AuthorizationPolicyBuilder RequirePermission(
            this AuthorizationPolicyBuilder builder, 
            string permission)
        {
            return builder.AddRequirements(new PermissionRequirement(permission));
        }
    }
}
