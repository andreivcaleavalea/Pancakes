using AdminService.Models.DTOs;

namespace AdminService.Models.Responses
{
    public class AdminLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public AdminUserDto AdminUser { get; set; } = new AdminUserDto();
        public DateTime ExpiresAt { get; set; }
        public bool RequirePasswordChange { get; set; }
        public bool RequireTwoFactor { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}