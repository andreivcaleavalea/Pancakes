using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? BlogTitle { get; set; }
        
        public string? BlogId { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        [Required]
        public string Source { get; set; } = string.Empty;
        
        public string? AdditionalData { get; set; }
    }
}

