namespace UserService.Models.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Ban status helper property (calculated from Bans collection)
    public bool IsBanned { get; set; } = false;
    public string? CurrentBanReason { get; set; }
    public DateTime? CurrentBanExpiresAt { get; set; }
    public DateTime? BannedAt { get; set; }
    public string? BannedBy { get; set; }
    public int BanHistoryCount { get; set; }
}
