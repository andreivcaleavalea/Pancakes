namespace AdminService.Clients.UserClient.DTOs;

public class UserOverviewDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActive { get; set; }
    public bool IsBanned { get; set; }
    public int PostCount { get; set; }
    public int CommentCount { get; set; }
}
