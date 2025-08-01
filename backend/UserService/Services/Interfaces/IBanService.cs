using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
using UserService.Models.Requests;

namespace UserService.Services.Interfaces;

public interface IBanService
{
    Task<BanDto?> GetActiveBanAsync(string userId);
    Task<IEnumerable<BanDto>> GetBanHistoryAsync(string userId);
    Task<BanDto> CreateBanAsync(BanUserRequest request);
    Task<BanDto> LiftBanAsync(string banId, string unbannedBy, string reason);
    Task<bool> HasActiveBanAsync(string userId);
    Task<int> GetActiveBansCountAsync();
    Task ProcessExpiredBansAsync();
    
    // HttpContext-aware methods for controller use
    Task<IActionResult> BanUserAsync(HttpContext httpContext, BanUserRequest request);
    Task<IActionResult> UnbanUserAsync(HttpContext httpContext, UnbanUserRequest request);
    Task<IActionResult> GetBanHistoryAsync(HttpContext httpContext, string userId);
}