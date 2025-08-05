namespace AdminService.Authorization
{
    /// Defines all available permissions in the admin system
    public static class AdminPermissions
    {
        // User Management Permissions
        public const string ViewUsers = "users:view";
        public const string BanUsers = "users:ban";
        public const string UnbanUsers = "users:unban";
        public const string ViewUserDetails = "users:details";
        public const string UpdateUsers = "users:update";

        // Blog Management Permissions
        public const string ViewBlogs = "blogs:view";
        public const string ViewBlogDetails = "blogs:details";
        public const string ManageBlogs = "blogs:manage";
        public const string DeleteBlogs = "blogs:delete";

        // Analytics Permissions
        public const string ViewAnalytics = "analytics:view";
        public const string ViewDashboard = "analytics:dashboard";

        // Audit Permissions
        public const string ViewAuditLogs = "audit:view";
    }
}