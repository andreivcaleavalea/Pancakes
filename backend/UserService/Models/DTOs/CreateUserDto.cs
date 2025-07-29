using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTOs;

public class CreateUserDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Image { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string ProviderUserId { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
}
