using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;
using UserService.Services;

namespace UserService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FriendshipsController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;
    private readonly CurrentUserService _currentUserService;

    public FriendshipsController(IFriendshipService friendshipService, CurrentUserService currentUserService)
    {
        _friendshipService = friendshipService;
        _currentUserService = currentUserService;
    }

    [HttpGet("friends")]
    public async Task<ActionResult<IEnumerable<FriendDto>>> GetFriends()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var friends = await _friendshipService.GetUserFriendsAsync(currentUserId);
            return Ok(friends);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving friends: {ex.Message}");
            return StatusCode(500, new { message = "Error retrieving friends" });
        }
    }

    [HttpGet("requests")]
    public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetPendingRequests()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var requests = await _friendshipService.GetPendingRequestsAsync(currentUserId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving friend requests: {ex.Message}");
            return StatusCode(500, new { message = "Error retrieving friend requests" });
        }
    }

    [HttpGet("available-users")]
    public async Task<ActionResult<IEnumerable<AvailableUserDto>>> GetAvailableUsers()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var availableUsers = await _friendshipService.GetAvailableUsersAsync(currentUserId);
            return Ok(availableUsers);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving available users: {ex.Message}");
            return StatusCode(500, new { message = "Error retrieving available users" });
        }
    }

    [HttpPost("send-request")]
    public async Task<ActionResult<FriendshipDto>> SendFriendRequest([FromBody] CreateFriendRequestDto request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var friendship = await _friendshipService.SendFriendRequestAsync(currentUserId, request.ReceiverId);
            return Ok(friendship);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending friend request: {ex.Message}");
            return StatusCode(500, new { message = "Error sending friend request" });
        }
    }

    [HttpPost("{friendshipId}/accept")]
    public async Task<ActionResult<FriendshipDto>> AcceptFriendRequest(Guid friendshipId)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var friendship = await _friendshipService.AcceptFriendRequestAsync(friendshipId, currentUserId);
            return Ok(friendship);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting friend request: {ex.Message}");
            return StatusCode(500, new { message = "Error accepting friend request" });
        }
    }

    [HttpPost("{friendshipId}/reject")]
    public async Task<ActionResult<FriendshipDto>> RejectFriendRequest(Guid friendshipId)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var friendship = await _friendshipService.RejectFriendRequestAsync(friendshipId, currentUserId);
            return Ok(friendship);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rejecting friend request: {ex.Message}");
            return StatusCode(500, new { message = "Error rejecting friend request" });
        }
    }

    [HttpDelete("remove/{friendUserId}")]
    public async Task<ActionResult> RemoveFriend(string friendUserId)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            await _friendshipService.RemoveFriendAsync(currentUserId, friendUserId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing friend: {ex.Message}");
            return StatusCode(500, new { message = "Error removing friend" });
        }
    }

    [HttpGet("check-friendship/{userId}")]
    public async Task<ActionResult<bool>> CheckFriendship(string userId)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var areFriends = await _friendshipService.AreFriendsAsync(currentUserId, userId);
            return Ok(areFriends);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking friendship: {ex.Message}");
            return StatusCode(500, new { message = "Error checking friendship" });
        }
    }
} 