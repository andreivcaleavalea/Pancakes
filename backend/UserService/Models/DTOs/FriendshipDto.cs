namespace UserService.Models.DTOs;

public class FriendshipDto
{
    public Guid Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    
    // User information will be populated by service
    public UserDto? SenderInfo { get; set; }
    public UserDto? ReceiverInfo { get; set; }
}

public class CreateFriendRequestDto
{
    public string ReceiverId { get; set; } = string.Empty;
}

public class FriendDto
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public DateTime FriendsSince { get; set; }
}

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderImage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AvailableUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Bio { get; set; }
} 