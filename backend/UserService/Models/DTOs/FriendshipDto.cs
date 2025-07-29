namespace UserService.Models.DTOs
{
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
} 