using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AdminService.Authorization
{
    /// Handles permission-based authorization requirements
    public class PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger) : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger = logger;

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
}