using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;

namespace AdminService.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<PagedResponse<UserOverviewDto>> SearchUsersAsync(UserSearchRequest request);
        Task<UserDetailDto?> GetUserDetailAsync(string userId);
        Task<bool> BanUserAsync(BanUserRequest request, string bannedBy);
        Task<bool> UnbanUserAsync(UnbanUserRequest request, string unbannedBy);
        Task<UserOverviewDto?> UpdateUserAsync(string userId, UpdateUserRequest request, string updatedBy);
        Task<bool> ForcePasswordResetAsync(ForcePasswordResetRequest request, string requestedBy);
        Task<bool> MergeUsersAsync(MergeUsersRequest request, string mergedBy);
        Task<bool> DeleteUserAsync(string userId, string reason, string deletedBy);
        Task<List<UserOverviewDto>> GetRecentlyRegisteredUsersAsync(int count = 10);
        Task<List<UserOverviewDto>> GetMostActiveUsersAsync(int count = 10);
        Task<List<UserOverviewDto>> GetSuspiciousUsersAsync();
        Task<Dictionary<string, object>> GetUserStatisticsAsync();
    }
}