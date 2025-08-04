namespace AdminService.Models.DTOs
{
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int AdminLevel { get; set; }
        public bool IsActive { get; set; }
        public bool RequirePasswordChange { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public DateTime PasswordChangedAt { get; set; }
        public List<AdminRoleDto> Roles { get; set; } = new List<AdminRoleDto>();
    }
}