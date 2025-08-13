using AdminService.Clients.UserClient.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;

namespace AdminService.Clients.UserClient
{
    public interface IUserServiceClient
    {
        Task<PagedResponse<UserOverviewDto>> SearchUsersAsync(UserSearchRequest request);
        Task<UserDetailDto?> GetUserDetailAsync(string userId);
        Task<bool> BanUserAsync(BanUserRequest request, string adminId);
        Task<bool> UnbanUserAsync(UnbanUserRequest request, string adminId);
        Task<UserDetailDto?> UpdateUserAsync(string userId, UpdateUserRequest request, string adminId);
        Task<bool> ForcePasswordResetAsync(ForcePasswordResetRequest request, string adminId);
        Task<Dictionary<string, object>> GetUserStatisticsAsync();
        Task<bool> CreateNotificationAsync(
            string userId, 
            string type, 
            string title, 
            string message, 
            string reason, 
            string source, 
            string? blogTitle = null, 
            string? blogId = null, 
            string? additionalData = null);
    }
}
