using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;

namespace BlogService.Controllers;

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
    public async Task<ActionResult<IEnumerable<FriendDto>>> GetFriends()
    {
        try
        {
            var friends = await _friendshipService.GetFriendsAsync(HttpContext);
            return Ok(friends);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving friends: {ex.Message}");
        }
    }

    [HttpGet("requests")]
    public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetPendingRequests()
    {
        try
        {
            var requests = await _friendshipService.GetPendingRequestsAsync(HttpContext);
            return Ok(requests);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving friend requests: {ex.Message}");
        }
    }

    [HttpGet("available-users")]
    public async Task<ActionResult<IEnumerable<UserInfoDto>>> GetAvailableUsers()
    {
        try
        {
            var availableUsers = await _friendshipService.GetAvailableUsersAsync(HttpContext);
            return Ok(availableUsers);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving available users: {ex.Message}");
        }
    }

    [HttpPost("send-request")]
    public async Task<ActionResult<FriendshipDto>> SendFriendRequest([FromBody] CreateFriendRequestDto request)
    {
        try
        {
            var friendship = await _friendshipService.SendFriendRequestAsync(request, HttpContext);
            return Ok(friendship);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error sending friend request: {ex.Message}");
        }
    }

    [HttpPost("{friendshipId}/accept")]
    public async Task<ActionResult<FriendshipDto>> AcceptFriendRequest(Guid friendshipId)
    {
        try
        {
            var friendship = await _friendshipService.AcceptFriendRequestAsync(friendshipId, HttpContext);
            return Ok(friendship);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error accepting friend request: {ex.Message}");
        }
    }

    [HttpPost("{friendshipId}/reject")]
    public async Task<ActionResult<FriendshipDto>> RejectFriendRequest(Guid friendshipId)
    {
        try
        {
            var friendship = await _friendshipService.RejectFriendRequestAsync(friendshipId, HttpContext);
            return Ok(friendship);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error rejecting friend request: {ex.Message}");
        }
    }

    [HttpDelete("remove/{friendUserId}")]
    public async Task<ActionResult> RemoveFriend(string friendUserId)
    {
        try
        {
            await _friendshipService.RemoveFriendAsync(friendUserId, HttpContext);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error removing friend: {ex.Message}");
        }
    }

    [HttpGet("check-friendship/{userId}")]
    public async Task<ActionResult<bool>> CheckFriendship(string userId)
    {
        try
        {
            var areFriends = await _friendshipService.CheckFriendshipAsync(userId, HttpContext);
            return Ok(areFriends);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error checking friendship: {ex.Message}");
        }
    }
} 