namespace UserService.Models.DTOs
{
    public class BanUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public string BannedBy { get; set; } = string.Empty;
    }
    
    public class UnbanUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string UnbannedBy { get; set; } = string.Empty;
    }
}
