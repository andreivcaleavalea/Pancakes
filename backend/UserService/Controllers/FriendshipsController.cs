using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FriendshipsController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendshipsController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        return await _friendshipService.GetFriendsAsync(HttpContext);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        return await _friendshipService.GetPendingRequestsAsync(HttpContext);
    }

    [HttpGet("available-users")]
    public async Task<IActionResult> GetAvailableUsers()
    {
        return await _friendshipService.GetAvailableUsersAsync(HttpContext);
    }

    [HttpPost("send-request")]
    public async Task<IActionResult> SendFriendRequest([FromBody] CreateFriendRequestDto request)
    {
        return await _friendshipService.SendFriendRequestAsync(HttpContext, request);
    }

    [HttpPost("{friendshipId}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(Guid friendshipId)
    {
        return await _friendshipService.AcceptFriendRequestAsync(HttpContext, friendshipId);
    }

    [HttpPost("{friendshipId}/reject")]
    public async Task<IActionResult> RejectFriendRequest(Guid friendshipId)
    {
        return await _friendshipService.RejectFriendRequestAsync(HttpContext, friendshipId);
    }

    [HttpDelete("remove/{friendUserId}")]
    public async Task<IActionResult> RemoveFriend(string friendUserId)
    {
        return await _friendshipService.RemoveFriendAsync(HttpContext, friendUserId);
    }

    [HttpGet("check-friendship/{userId}")]
    public async Task<IActionResult> CheckFriendship(string userId)
    {
        return await _friendshipService.CheckFriendshipAsync(HttpContext, userId);
    }
} 