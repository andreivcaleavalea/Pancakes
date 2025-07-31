using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Entities
{
    public class SystemConfiguration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        public string Value { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Category { get; set; } = "general"; // general, security, features, etc.
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string DataType { get; set; } = "string"; // string, int, bool, json
        
        public bool IsSecret { get; set; } = false; // For sensitive configurations
        
        [MaxLength(450)]
        public string? UpdatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}