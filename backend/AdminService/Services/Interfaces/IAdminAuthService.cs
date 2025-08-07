using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using Microsoft.AspNetCore.Http;

namespace AdminService.Services.Interfaces
{
    public interface IAdminAuthService
    {
        Task<AdminLoginResponse> LoginAsync(HttpContext httpContext, AdminLoginRequest request);
        Task<AdminUserDto> GetCurrentAdminAsync(string token);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> HasAdminUsersAsync();
        Task<AdminUserDto> CreateBootstrapAdminAsync(CreateAdminUserRequest request);
        string GenerateJwtToken(AdminUserDto admin);
        bool VerifyPassword(string password, string hashedPassword);
        string HashPassword(string password);
    }
}