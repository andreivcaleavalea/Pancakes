namespace UserService.Models.DTOs
{
    public class AvailableUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? Bio { get; set; }
    }
}
