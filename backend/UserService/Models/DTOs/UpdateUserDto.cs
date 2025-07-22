using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTOs;

public class UpdateUserDto
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    
    [MaxLength(500)]
    public string? Image { get; set; }
    
    [MaxLength(1000)]
    public string? Bio { get; set; }
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
}
