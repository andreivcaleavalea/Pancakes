namespace AdminService.Authorization
{
    /// Defines all available permissions in the admin system
    public static class AdminPermissions
    {
        // User Management Permissions
        public const string ViewUsers = "users:view";
        public const string BanUsers = "users:ban";
        public const string UnbanUsers = "users:unban";
        public const string DeleteUsers = "users:delete";
        public const string ViewUserDetails = "users:details";

        // Admin Management Permissions
        public const string ViewAdmins = "admins:view";
        public const string CreateAdmins = "admins:create";
        public const string UpdateAdmins = "admins:update";
        public const string DeleteAdmins = "admins:delete";
        public const string ManageRoles = "admins:roles";

        // Content Moderation Permissions
        public const string ViewContent = "content:view";
        public const string ModerateContent = "content:moderate";
        public const string DeleteContent = "content:delete";
        public const string ViewReports = "content:reports";

        // Analytics Permissions
        public const string ViewAnalytics = "analytics:view";
        public const string ViewDashboard = "analytics:dashboard";
        public const string ExportData = "analytics:export";

        // System Configuration Permissions
        public const string ViewSystemConfig = "system:view";
        public const string UpdateSystemConfig = "system:update";
        public const string ViewLogs = "system:logs";
        public const string ManageBackups = "system:backups";

        // Audit Permissions
        public const string ViewAuditLogs = "audit:view";
        public const string ExportAuditLogs = "audit:export";

        /// Gets all permissions for a given category
        public static class Categories
        {
            public static readonly string[] UserManagement = {
                ViewUsers, BanUsers, UnbanUsers, DeleteUsers, ViewUserDetails
            };

            public static readonly string[] AdminManagement = {
                ViewAdmins, CreateAdmins, UpdateAdmins, DeleteAdmins, ManageRoles
            };

            public static readonly string[] ContentModeration = {
                ViewContent, ModerateContent, DeleteContent, ViewReports
            };

            public static readonly string[] Analytics = {
                ViewAnalytics, ViewDashboard, ExportData
            };

            public static readonly string[] SystemConfiguration = {
                ViewSystemConfig, UpdateSystemConfig, ViewLogs, ManageBackups
            };

            public static readonly string[] Audit = {
                ViewAuditLogs, ExportAuditLogs
            };
        }

        /// Predefined role permissions
        public static class RolePermissions
        {
            public static readonly string[] SuperAdmin = Categories.UserManagement
                .Concat(Categories.AdminManagement)
                .Concat(Categories.ContentModeration)
                .Concat(Categories.Analytics)
                .Concat(Categories.SystemConfiguration)
                .Concat(Categories.Audit)
                .ToArray();

            public static readonly string[] Admin = Categories.UserManagement
                .Concat(Categories.ContentModeration)
                .Concat(Categories.Analytics)
                .Concat(new[] { ViewAdmins, ViewSystemConfig, ViewAuditLogs })
                .ToArray();

            public static readonly string[] Moderator = Categories.ContentModeration
                .Concat(new[] { ViewUsers, ViewUserDetails, ViewAnalytics, ViewDashboard })
                .ToArray();

            public static readonly string[] Viewer = new[]
            {
                ViewUsers, ViewUserDetails, ViewContent, ViewAnalytics, ViewDashboard
            };
        }
    }
}