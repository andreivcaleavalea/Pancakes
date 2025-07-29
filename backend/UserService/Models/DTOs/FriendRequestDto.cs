namespace UserService.Models.DTOs
{
    public class FriendRequestDto
    {
        public Guid Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
