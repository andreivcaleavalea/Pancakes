namespace UserService.Models.DTOs
{
    public class FriendDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Image { get; set; }
        public DateTime FriendsSince { get; set; }
    }
}
