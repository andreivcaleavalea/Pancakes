using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class User
    {
        [Required]
        [MaxLength(255)]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Image { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string ProviderUserId { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Bio { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public DateTime? DateOfBirth { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LoginRequest
    {
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public User User { get; set; } = new User();
        public DateTime ExpiresAt { get; set; }
    }

    public class OAuthUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }
}