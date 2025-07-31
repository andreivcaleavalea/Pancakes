using AdminService.Models.DTOs;

namespace AdminService.Models.Responses
{
    public class AdminLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public AdminUserDto AdminUser { get; set; } = new AdminUserDto();
        public DateTime ExpiresAt { get; set; }
        public bool RequirePasswordChange { get; set; }
        public bool RequireTwoFactor { get; set; }
    }
}