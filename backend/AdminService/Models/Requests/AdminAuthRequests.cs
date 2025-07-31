using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Requests
{
    public class AdminLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        public string? TwoFactorCode { get; set; }
    }

    public class CreateAdminUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 4)]
        public int AdminLevel { get; set; } = 1;
        
        public List<Guid> RoleIds { get; set; } = new List<Guid>();
        
        public bool RequirePasswordChange { get; set; } = true;
    }

    public class UpdateAdminUserRequest
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Range(1, 4)]
        public int AdminLevel { get; set; }
        
        public bool IsActive { get; set; }
        
        public List<Guid> RoleIds { get; set; } = new List<Guid>();
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string AdminId { get; set; } = string.Empty;
        
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
        
        public bool RequirePasswordChange { get; set; } = true;
    }
}