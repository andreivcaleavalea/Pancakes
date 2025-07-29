using UserService.Models.Entities;

namespace UserService.Models.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public User User { get; set; } = new User();
        public DateTime ExpiresAt { get; set; }
    }
}
