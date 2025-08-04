using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Models.Requests;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class BanService : IBanService
{
    private readonly IBanRepository _banRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BanService> _logger;

    public BanService(
        IBanRepository banRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<BanService> logger)
    {
        _banRepository = banRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BanDto?> GetActiveBanAsync(string userId)
    {
        var ban = await _banRepository.GetActiveBanAsync(userId);
        return ban != null ? _mapper.Map<BanDto>(ban) : null;
    }

    public async Task<IEnumerable<BanDto>> GetBanHistoryAsync(string userId)
    {
        var bans = await _banRepository.GetBanHistoryAsync(userId);
        return _mapper.Map<IEnumerable<BanDto>>(bans);
    }

    public async Task<BanDto> CreateBanAsync(BanUserRequest request)
    {
        var ban = new Ban
        {
            UserId = request.UserId,
            Reason = request.Reason,
            BannedAt = DateTime.UtcNow,
            BannedBy = request.BannedBy,
            ExpiresAt = request.ExpiresAt,
            IsActive = true
        };

        var createdBan = await _banRepository.CreateAsync(ban);
        _logger.LogInformation("User {UserId} banned by {BannedBy} for reason: {Reason}", 
            request.UserId, request.BannedBy, request.Reason);

        return _mapper.Map<BanDto>(createdBan);
    }

    public async Task<BanDto> LiftBanAsync(string banId, string unbannedBy, string reason)
    {
        var ban = await _banRepository.GetByIdAsync(banId);
        if (ban == null)
            throw new ArgumentException($"Ban with ID {banId} not found");

        ban.IsActive = false;
        ban.UnbannedAt = DateTime.UtcNow;
        ban.UnbannedBy = unbannedBy;
        ban.UnbanReason = reason;

        var updatedBan = await _banRepository.UpdateAsync(ban);
        _logger.LogInformation("Ban {BanId} lifted by {UnbannedBy} for reason: {Reason}", 
            banId, unbannedBy, reason);

        return _mapper.Map<BanDto>(updatedBan);
    }

    public async Task<bool> HasActiveBanAsync(string userId)
    {
        return await _banRepository.HasActiveBanAsync(userId);
    }

    public async Task<int> GetActiveBansCountAsync()
    {
        return await _banRepository.GetActiveBansCountAsync();
    }

    public async Task ProcessExpiredBansAsync()
    {
        var expiredBans = await _banRepository.GetExpiringBansAsync(DateTime.UtcNow);
        
        foreach (var ban in expiredBans)
        {
            ban.IsActive = false;
            ban.UnbannedAt = DateTime.UtcNow;
            ban.UnbannedBy = "System";
            ban.UnbanReason = "Ban expired automatically";
            
            await _banRepository.UpdateAsync(ban);
            _logger.LogInformation("Automatically lifted expired ban {BanId} for user {UserId}", 
                ban.Id, ban.UserId);
        }
    }

    public async Task<IActionResult> BanUserAsync(HttpContext httpContext, BanUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return new NotFoundObjectResult(new { message = "User not found" });
            }

            // Check if user already has an active ban
            if (await HasActiveBanAsync(request.UserId))
            {
                return new BadRequestObjectResult(new { message = "User already has an active ban" });
            }

            var banDto = await CreateBanAsync(request);
            return new OkObjectResult(new { message = "User banned successfully", ban = banDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ban user error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> UnbanUserAsync(HttpContext httpContext, UnbanUserRequest request)
    {
        try
        {
            var activeBan = await _banRepository.GetActiveBanAsync(request.UserId);
            if (activeBan == null)
            {
                return new BadRequestObjectResult(new { message = "User does not have an active ban" });
            }

            var banDto = await LiftBanAsync(activeBan.Id, request.UnbannedBy, request.Reason);
            return new OkObjectResult(new { message = "User unbanned successfully", ban = banDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unban user error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> GetBanHistoryAsync(HttpContext httpContext, string userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new NotFoundObjectResult(new { message = "User not found" });
            }

            var banHistory = await GetBanHistoryAsync(userId);
            return new OkObjectResult(banHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get ban history error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }
}