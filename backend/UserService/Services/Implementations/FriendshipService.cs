using AutoMapper;
using UserService.Models;
using UserService.Models.DTOs;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;
using UserService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UserService.Services.Implementations;

public class FriendshipService : IFriendshipService
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<FriendshipService> _logger;

    public FriendshipService(
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<FriendshipService> logger)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<IEnumerable<FriendDto>> GetUserFriendsAsync(string userId)
    {
        var friendships = await _friendshipRepository.GetUserFriendsAsync(userId);
        var friends = new List<FriendDto>();

        foreach (var friendship in friendships)
        {
            var friendId = friendship.SenderId == userId ? friendship.ReceiverId : friendship.SenderId;
            var friendUser = await _userRepository.GetByIdAsync(friendId);
            
            if (friendUser != null)
            {
                friends.Add(new FriendDto
                {
                    UserId = friendUser.Id,
                    Name = friendUser.Name,
                    Image = friendUser.Image,
                    FriendsSince = friendship.AcceptedAt ?? friendship.CreatedAt
                });
            }
        }

        return friends;
    }

    public async Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(string userId)
    {
        var requests = await _friendshipRepository.GetPendingRequestsReceivedAsync(userId);
        var friendRequests = new List<FriendRequestDto>();

        foreach (var request in requests)
        {
            var senderUser = await _userRepository.GetByIdAsync(request.SenderId);
            
            if (senderUser != null)
            {
                friendRequests.Add(new FriendRequestDto
                {
                    Id = request.Id,
                    SenderId = request.SenderId,
                    SenderName = senderUser.Name,
                    SenderImage = senderUser.Image,
                    CreatedAt = request.CreatedAt
                });
            }
        }

        return friendRequests;
    }

    public async Task<IEnumerable<AvailableUserDto>> GetAvailableUsersAsync(string userId)
    {
        // Get all users
        var allUsers = await _userRepository.GetAllAsync(1, 1000); // Get a large number for now

        // Get friend IDs and pending request user IDs
        var friendUserIds = await _friendshipRepository.GetFriendUserIdsAsync(userId);
        var pendingRequestsSent = await _friendshipRepository.GetPendingRequestsSentAsync(userId);
        var pendingRequestsReceived = await _friendshipRepository.GetPendingRequestsReceivedAsync(userId);

        var excludedUserIds = new HashSet<string>(friendUserIds)
        {
            userId // Exclude current user
        };

        // Add users with pending requests (both sent and received)
        foreach (var request in pendingRequestsSent)
        {
            excludedUserIds.Add(request.ReceiverId);
        }
        foreach (var request in pendingRequestsReceived)
        {
            excludedUserIds.Add(request.SenderId);
        }

        // Filter out excluded users
        var availableUsers = allUsers.Where(user => !excludedUserIds.Contains(user.Id));

        return availableUsers.Select(user => new AvailableUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Image = user.Image,
            Bio = user.Bio
        });
    }

    public async Task<FriendshipDto> SendFriendRequestAsync(string senderId, string receiverId)
    {
        if (senderId == receiverId)
        {
            throw new ArgumentException("Cannot send friend request to yourself");
        }

        // Check if friendship already exists
        var existingFriendship = await _friendshipRepository.GetFriendshipAsync(senderId, receiverId);
        if (existingFriendship != null)
        {
            throw new InvalidOperationException("Friendship or friend request already exists");
        }

        // Verify that receiver exists
        var receiverUser = await _userRepository.GetByIdAsync(receiverId);
        if (receiverUser == null)
        {
            throw new ArgumentException("Receiver user not found");
        }

        var friendship = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendshipStatus.Pending
        };

        var createdFriendship = await _friendshipRepository.CreateAsync(friendship);
        return _mapper.Map<FriendshipDto>(createdFriendship);
    }

    public async Task<FriendshipDto> AcceptFriendRequestAsync(Guid friendshipId, string userId)
    {
        var friendship = await _friendshipRepository.GetByIdAsync(friendshipId);
        if (friendship == null)
        {
            throw new ArgumentException("Friend request not found");
        }

        if (friendship.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("You can only accept friend requests sent to you");
        }

        if (friendship.Status != FriendshipStatus.Pending)
        {
            throw new InvalidOperationException("Friend request is not pending");
        }

        friendship.Status = FriendshipStatus.Accepted;
        friendship.AcceptedAt = DateTime.UtcNow;

        var updatedFriendship = await _friendshipRepository.UpdateAsync(friendship);
        return _mapper.Map<FriendshipDto>(updatedFriendship);
    }

    public async Task<FriendshipDto> RejectFriendRequestAsync(Guid friendshipId, string userId)
    {
        var friendship = await _friendshipRepository.GetByIdAsync(friendshipId);
        if (friendship == null)
        {
            throw new ArgumentException("Friend request not found");
        }

        if (friendship.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("You can only reject friend requests sent to you");
        }

        if (friendship.Status != FriendshipStatus.Pending)
        {
            throw new InvalidOperationException("Friend request is not pending");
        }

        friendship.Status = FriendshipStatus.Rejected;

        var updatedFriendship = await _friendshipRepository.UpdateAsync(friendship);
        return _mapper.Map<FriendshipDto>(updatedFriendship);
    }

    public async Task RemoveFriendAsync(string userId, string friendUserId)
    {
        var friendship = await _friendshipRepository.GetFriendshipAsync(userId, friendUserId);
        if (friendship == null)
        {
            throw new ArgumentException("Friendship not found");
        }

        if (friendship.Status != FriendshipStatus.Accepted)
        {
            throw new ArgumentException("Cannot remove a user who is not your friend");
        }

        await _friendshipRepository.DeleteAsync(friendship.Id);
    }

    public async Task<bool> AreFriendsAsync(string userId1, string userId2)
    {
        return await _friendshipRepository.AreFriendsAsync(userId1, userId2);
    }

    // HttpContext-aware methods for controller use
    public async Task<IActionResult> GetFriendsAsync(HttpContext httpContext)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return new UnauthorizedObjectResult("User not authenticated");
            }

            var friends = await GetUserFriendsAsync(currentUserId);
            return new OkObjectResult(friends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving friends");
            return new ObjectResult(new { message = "Error retrieving friends" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> GetPendingRequestsAsync(HttpContext httpContext)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return new UnauthorizedObjectResult("User not authenticated");
            }

            var requests = await GetPendingRequestsAsync(currentUserId);
            return new OkObjectResult(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving friend requests");
            return new ObjectResult(new { message = "Error retrieving friend requests" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> GetAvailableUsersAsync(HttpContext httpContext)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return new UnauthorizedObjectResult("User not authenticated");
            }

            var availableUsers = await GetAvailableUsersAsync(currentUserId);
            return new OkObjectResult(availableUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available users");
            return new ObjectResult(new { message = "Error retrieving available users" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> SendFriendRequestAsync(HttpContext httpContext, CreateFriendRequestDto request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return new UnauthorizedObjectResult("User not authenticated");
            }

            var friendship = await SendFriendRequestAsync(currentUserId, request.ReceiverId);
            return new OkObjectResult(friendship);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return new ConflictObjectResult(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request");
            return new ObjectResult(new { message = "Error sending friend request" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> AcceptFriendRequestAsync(HttpContext httpContext, Guid friendshipId)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return new UnauthorizedObjectResult("User not authenticated");
            }

            var friendship = await AcceptFriendRequestAsync(friendshipId, currentUserId);
            return new OkObjectResult(friendship);
        }
        catch (ArgumentException ex)
        {
            return new NotFoundObjectResult(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ForbidResult(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting friend request");
            return new ObjectResult(new { message = "Error accepting friend request" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> RejectFriendRequestAsync(HttpContext httpContext, Guid friendshipId)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return new UnauthorizedObjectResult("User not authenticated");
            }

            var friendship = await RejectFriendRequestAsync(friendshipId, currentUserId);
            return new OkObjectResult(friendship);
        }
        catch (ArgumentException ex)
        {
            return new NotFoundObjectResult(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ForbidResult(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting friend request");
            return new ObjectResult(new { message = "Error rejecting friend request" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> RemoveFriendAsync(HttpContext httpContext, string friendUserId)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return new UnauthorizedObjectResult("User not authenticated");
            }

            await RemoveFriendAsync(currentUserId, friendUserId);
            return new NoContentResult();
        }
        catch (ArgumentException ex)
        {
            return new NotFoundObjectResult(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend");
            return new ObjectResult(new { message = "Error removing friend" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> CheckFriendshipAsync(HttpContext httpContext, string userId)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return new UnauthorizedObjectResult("User not authenticated");
            }

            var areFriends = await AreFriendsAsync(currentUserId, userId);
            return new OkObjectResult(areFriends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking friendship");
            return new ObjectResult(new { message = "Error checking friendship" }) { StatusCode = 500 };
        }
    }
} 