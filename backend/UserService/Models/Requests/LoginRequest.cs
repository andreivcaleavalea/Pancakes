namespace UserService.Models.Requests
{
    public class LoginRequest
    {
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }
}
