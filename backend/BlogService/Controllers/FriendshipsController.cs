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
    private readonly IJwtUserService _jwtUserService;
    private readonly IUserServiceClient _userServiceClient;

    public FriendshipsController(IFriendshipService friendshipService, IJwtUserService jwtUserService, IUserServiceClient userServiceClient)
    {
        _friendshipService = friendshipService;
        _jwtUserService = jwtUserService;
        _userServiceClient = userServiceClient;
    }

    [HttpGet("friends")]
    public async Task<ActionResult<IEnumerable<FriendDto>>> GetFriends()
    {
        try
        {
            var currentUserId = _jwtUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var friends = await _friendshipService.GetUserFriendsAsync(currentUserId);
            return Ok(friends);
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
            var currentUserId = _jwtUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var requests = await _friendshipService.GetPendingRequestsAsync(currentUserId);
            return Ok(requests);
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
            var currentUserId = _jwtUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var availableUsers = await _friendshipService.GetAvailableUsersAsync(currentUserId);
            return Ok(availableUsers);
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
            var currentUserId = _jwtUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var friendship = await _friendshipService.SendFriendRequestAsync(currentUserId, request.ReceiverId);
            return Ok(friendship);
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
            var currentUserId = _jwtUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var friendship = await _friendshipService.AcceptFriendRequestAsync(friendshipId, currentUserId);
            return Ok(friendship);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
            var currentUserId = _jwtUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var friendship = await _friendshipService.RejectFriendRequestAsync(friendshipId, currentUserId);
            return Ok(friendship);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
            var currentUserId = _jwtUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            await _friendshipService.RemoveFriendAsync(currentUserId, friendUserId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
            var currentUserId = _jwtUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var areFriends = await _friendshipService.AreFriendsAsync(currentUserId, userId);
            return Ok(areFriends);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error checking friendship: {ex.Message}");
        }
    }
} 