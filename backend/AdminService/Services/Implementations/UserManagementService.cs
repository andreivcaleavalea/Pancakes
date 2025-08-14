using AdminService.Clients.UserClient;
using AdminService.Clients.UserClient.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AdminService.Validations;

namespace AdminService.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserServiceClient _userServiceClient;
        private readonly IAuditService _auditService;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            IUserServiceClient userServiceClient,
            IAuditService auditService,
            ILogger<UserManagementService> logger)
        {
            _userServiceClient = userServiceClient;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ServiceResult<PagedResponse<UserOverviewDto>>> SearchUsersAsync(UserSearchRequest request)
        {
            try
            {
                var users = await _userServiceClient.SearchUsersAsync(request);
                return ServiceResult<PagedResponse<UserOverviewDto>>.SuccessResult(users, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return ServiceResult<PagedResponse<UserOverviewDto>>.FailureResult(
                    "An error occurred while searching users", ex.Message);
            }
        }

        public async Task<ServiceResult<UserDetailDto>> GetUserDetailAsync(string userId)
        {
            try
            {
                var user = await _userServiceClient.GetUserDetailAsync(userId);
                if (user == null)
                {
                    return ServiceResult<UserDetailDto>.FailureResult("User not found");
                }

                return ServiceResult<UserDetailDto>.SuccessResult(user, "User details retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for {UserId}", userId);
                return ServiceResult<UserDetailDto>.FailureResult(
                    "An error occurred while retrieving user details", ex.Message);
            }
        }

        public async Task<ServiceResult<string>> BanUserAsync(BanUserRequest request, string adminId, string ipAddress, string userAgent)
        {
            try
            {
                var validationResult = BanRequestValidator.ValidateBanRequest(request);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<string>.FailureResult("Validation failed", validationResult.Errors.ToList());
                }

                var success = await _userServiceClient.BanUserAsync(request, adminId);

                if (!success)
                {
                    return ServiceResult<string>.FailureResult("Failed to ban user");
                }

                await _auditService.LogActionAsync(adminId, "USER_BANNED", "User", request.UserId,
                    request, ipAddress, userAgent);

                return ServiceResult<string>.SuccessResult("User banned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user {UserId}", request.UserId);
                return ServiceResult<string>.FailureResult(
                    "An error occurred while banning user", ex.Message);
            }
        }

        public async Task<ServiceResult<string>> UnbanUserAsync(UnbanUserRequest request, string adminId, string ipAddress, string userAgent)
        {
            try
            {
                var validationResult = UnbanRequestValidator.ValidateUnbanRequest(request);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<string>.FailureResult("Validation failed", validationResult.Errors.ToList());
                }

                var success = await _userServiceClient.UnbanUserAsync(request, adminId);

                if (!success)
                {
                    return ServiceResult<string>.FailureResult("Failed to unban user");
                }

                await _auditService.LogActionAsync(adminId, "USER_UNBANNED", "User", request.UserId,
                    request, ipAddress, userAgent);

                return ServiceResult<string>.SuccessResult("User unbanned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user {UserId}", request.UserId);
                return ServiceResult<string>.FailureResult(
                    "An error occurred while unbanning user", ex.Message);
            }
        }

        public async Task<ServiceResult<UserDetailDto>> UpdateUserAsync(string userId, UpdateUserRequest request, string adminId, string ipAddress, string userAgent)
        {
            try
            {
                var user = await _userServiceClient.UpdateUserAsync(userId, request, adminId);

                if (user == null)
                {
                    return ServiceResult<UserDetailDto>.FailureResult("User not found");
                }

                await _auditService.LogActionAsync(adminId, "USER_UPDATED", "User", userId,
                    request, ipAddress, userAgent);

                return ServiceResult<UserDetailDto>.SuccessResult(user, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return ServiceResult<UserDetailDto>.FailureResult(
                    "An error occurred while updating user", ex.Message);
            }
        }

        public async Task<ServiceResult<string>> ForcePasswordResetAsync(ForcePasswordResetRequest request, string adminId, string ipAddress, string userAgent)
        {
            try
            {
                var success = await _userServiceClient.ForcePasswordResetAsync(request, adminId);

                await _auditService.LogActionAsync(adminId, "FORCE_PASSWORD_RESET", "User", request.UserId,
                    request, ipAddress, userAgent);

                return ServiceResult<string>.SuccessResult("Password reset initiated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forcing password reset for user {UserId}", request.UserId);
                return ServiceResult<string>.FailureResult(
                    "An error occurred while initiating password reset", ex.Message);
            }
        }

        public async Task<ServiceResult<Dictionary<string, object>>> GetUserStatisticsAsync()
        {
            try
            {
                var stats = await _userServiceClient.GetUserStatisticsAsync();
                return ServiceResult<Dictionary<string, object>>.SuccessResult(stats, "User statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return ServiceResult<Dictionary<string, object>>.FailureResult(
                    "An error occurred while retrieving user statistics", ex.Message);
            }
        }
    }
}
