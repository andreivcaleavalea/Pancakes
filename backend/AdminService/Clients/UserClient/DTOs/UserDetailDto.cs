namespace AdminService.Clients.UserClient.DTOs;

public class UserDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActive { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanDate { get; set; }
    public string? BannedBy { get; set; }
    public int PostCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public List<string> Roles { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
