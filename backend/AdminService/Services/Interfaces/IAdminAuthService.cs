using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;

namespace AdminService.Services.Interfaces
{
    public interface IAdminAuthService
    {
        Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request);
        Task<AdminUserDto> GetCurrentAdminAsync(string token);
        Task<AdminUserDto> CreateAdminUserAsync(CreateAdminUserRequest request, string createdBy);
        Task<AdminUserDto> UpdateAdminUserAsync(string adminId, UpdateAdminUserRequest request, string updatedBy);
        Task<bool> ChangePasswordAsync(string adminId, ChangePasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request, string resetBy);
        Task<bool> DeactivateAdminAsync(string adminId, string deactivatedBy);
        Task<bool> ActivateAdminAsync(string adminId, string activatedBy);
        Task<PagedResponse<AdminUserDto>> GetAdminUsersAsync(int page, int pageSize);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> HasAdminUsersAsync();
        Task<AdminUserDto> CreateBootstrapAdminAsync(CreateAdminUserRequest request);
        string GenerateJwtToken(AdminUserDto admin);
        bool VerifyPassword(string password, string hashedPassword);
        string HashPassword(string password);
    }
}