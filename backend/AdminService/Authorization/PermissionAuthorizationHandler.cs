using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AdminService.Authorization
{
    /// Handles permission-based authorization
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var user = context.User;
            
            if (!user.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("Unauthenticated user attempted to access protected resource");
                context.Fail();
                return Task.CompletedTask;
            }

            var hasPermission = user.HasClaim("permission", requirement.Permission);
            
            if (!hasPermission)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
                
                _logger.LogWarning("User {UserId} ({Email}) denied access. Missing permission: {Permission}", 
                    userId, userEmail, requirement.Permission);
                
                context.Fail();
                return Task.CompletedTask;
            }

            _logger.LogDebug("User {UserId} granted access with permission: {Permission}", 
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value, requirement.Permission);
            
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    /// Represents a permission requirement
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }

    /// Extension methods for easier policy configuration
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequirePermission(
            this AuthorizationPolicyBuilder builder, 
            string permission)
        {
            return builder.AddRequirements(new PermissionRequirement(permission));
        }
    }
}