namespace UserService.Models.DTOs
{
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? BlogTitle { get; set; }
        public string? BlogId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? AdditionalData { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}

