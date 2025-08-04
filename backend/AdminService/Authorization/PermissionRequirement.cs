using Microsoft.AspNetCore.Authorization;

namespace AdminService.Authorization
{
    /// Represents a permission requirement for authorization policies
    public class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}
