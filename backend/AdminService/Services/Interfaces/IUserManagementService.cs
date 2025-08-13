using AdminService.Clients.UserClient.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;

namespace AdminService.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<ServiceResult<PagedResponse<UserOverviewDto>>> SearchUsersAsync(UserSearchRequest request);
        Task<ServiceResult<UserDetailDto>> GetUserDetailAsync(string userId);
        Task<ServiceResult<string>> BanUserAsync(BanUserRequest request, string adminId, string ipAddress, string userAgent);
        Task<ServiceResult<string>> UnbanUserAsync(UnbanUserRequest request, string adminId, string ipAddress, string userAgent);
        Task<ServiceResult<UserDetailDto>> UpdateUserAsync(string userId, UpdateUserRequest request, string adminId, string ipAddress, string userAgent);
        Task<ServiceResult<string>> ForcePasswordResetAsync(ForcePasswordResetRequest request, string adminId, string ipAddress, string userAgent);
        Task<ServiceResult<Dictionary<string, object>>> GetUserStatisticsAsync();
    }
}
