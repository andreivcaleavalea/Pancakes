namespace UserService.Models.DTOs;

public class BanDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime BannedAt { get; set; }
    public string BannedBy { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? UnbannedAt { get; set; }
    public string? UnbannedBy { get; set; }
    public string? UnbanReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // User details for display
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}