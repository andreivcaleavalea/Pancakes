using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Requests
{
    public class CreateSystemConfigurationRequest
    {
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        public string Value { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Category { get; set; } = "general";
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string DataType { get; set; } = "string";
        
        public bool IsSecret { get; set; } = false;
    }

    public class UpdateSystemConfigurationRequest
    {
        [Required]
        public string Value { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Category { get; set; } = "general";
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string DataType { get; set; } = "string";
        
        public bool IsSecret { get; set; } = false;
    }
}
