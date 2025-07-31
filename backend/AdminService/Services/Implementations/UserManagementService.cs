using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AdminService.Clients;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private readonly AdminDbContext _context;
        private readonly UserServiceClient _userServiceClient;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            AdminDbContext context,
            UserServiceClient userServiceClient,
            IAuditService auditService,
            IMapper mapper,
            ILogger<UserManagementService> logger)
        {
            _context = context;
            _userServiceClient = userServiceClient;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<UserOverviewDto>> SearchUsersAsync(UserSearchRequest request)
        {
            try
            {
                var users = await _userServiceClient.SearchUsersAsync(request.SearchTerm, request.Page, request.PageSize);
                
                return new PagedResponse<UserOverviewDto>
                {
                    Data = users,
                    TotalCount = users.Count,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)users.Count / request.PageSize),
                    HasNext = request.Page * request.PageSize < users.Count,
                    HasPrevious = request.Page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                throw;
            }
        }

        public async Task<UserDetailDto?> GetUserDetailAsync(string userId)
        {
            return await _userServiceClient.GetUserDetailAsync(userId);
        }

        public async Task<bool> BanUserAsync(BanUserRequest request, string bannedBy)
        {
            try
            {
                var success = await _userServiceClient.BanUserAsync(request.UserId, request.Reason, request.ExpiresAt);
                
                if (success)
                {
                    await _auditService.LogActionAsync(bannedBy, "USER_BANNED", "User", request.UserId,
                        new { request.Reason, request.ExpiresAt, request.BanEmail, request.DeleteContent }, "", "");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user {UserId}", request.UserId);
                return false;
            }
        }

        public async Task<bool> UnbanUserAsync(UnbanUserRequest request, string unbannedBy)
        {
            try
            {
                var success = await _userServiceClient.UnbanUserAsync(request.UserId, request.Reason);
                
                if (success)
                {
                    await _auditService.LogActionAsync(unbannedBy, "USER_UNBANNED", "User", request.UserId,
                        new { request.Reason }, "", "");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user {UserId}", request.UserId);
                return false;
            }
        }

        public async Task<UserOverviewDto?> UpdateUserAsync(string userId, UpdateUserRequest request, string updatedBy)
        {
            try
            {
                await _auditService.LogActionAsync(updatedBy, "USER_UPDATED", "User", userId, request, "", "");
                
                // For now, return basic info - would need UserService endpoint to actually update
                return await _userServiceClient.GetUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> ForcePasswordResetAsync(ForcePasswordResetRequest request, string requestedBy)
        {
            await _auditService.LogActionAsync(requestedBy, "FORCE_PASSWORD_RESET", "User", request.UserId,
                new { request.Reason, request.SendEmail }, "", "");
            return true; // Would integrate with UserService
        }

        public async Task<bool> MergeUsersAsync(MergeUsersRequest request, string mergedBy)
        {
            await _auditService.LogActionAsync(mergedBy, "USERS_MERGED", "User", request.PrimaryUserId,
                new { request.SecondaryUserId, request.Reason }, "", "");
            return true; // Would integrate with UserService
        }

        public async Task<bool> DeleteUserAsync(string userId, string reason, string deletedBy)
        {
            await _auditService.LogActionAsync(deletedBy, "USER_DELETED", "User", userId,
                new { reason }, "", "");
            return true; // Would integrate with UserService
        }

        public async Task<List<UserOverviewDto>> GetRecentlyRegisteredUsersAsync(int count = 10)
        {
            return await _userServiceClient.SearchUsersAsync(null, 1, count);
        }

        public async Task<List<UserOverviewDto>> GetMostActiveUsersAsync(int count = 10)
        {
            return await _userServiceClient.SearchUsersAsync(null, 1, count);
        }

        public async Task<List<UserOverviewDto>> GetSuspiciousUsersAsync()
        {
            return new List<UserOverviewDto>(); // Would implement detection logic
        }

        public async Task<Dictionary<string, object>> GetUserStatisticsAsync()
        {
            return await _userServiceClient.GetUserStatisticsAsync();
        }
    }
}