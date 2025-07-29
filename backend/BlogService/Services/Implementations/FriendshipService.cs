using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class FriendshipService : IFriendshipService
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IMapper _mapper;
    private readonly IJwtUserService _jwtUserService;
    private readonly ILogger<FriendshipService> _logger;

    public FriendshipService(
        IFriendshipRepository friendshipRepository,
        IUserServiceClient userServiceClient,
        IMapper mapper,
        IJwtUserService jwtUserService,
        ILogger<FriendshipService> logger)
    {
        _friendshipRepository = friendshipRepository;
        _userServiceClient = userServiceClient;
        _mapper = mapper;
        _jwtUserService = jwtUserService;
        _logger = logger;
    }

    public async Task<IEnumerable<FriendDto>> GetUserFriendsAsync(string userId)
    {
        var friendships = await _friendshipRepository.GetUserFriendsAsync(userId);
        var friends = new List<FriendDto>();

        foreach (var friendship in friendships)
        {
            // Get the friend's user ID (the other person in the friendship)
            var friendId = friendship.SenderId == userId ? friendship.ReceiverId : friendship.SenderId;
            
            // Get user info from UserService
            var userInfo = await _userServiceClient.GetUserByIdAsync(friendId);
            if (userInfo != null)
            {
                friends.Add(new FriendDto
                {
                    UserId = friendId,
                    Name = userInfo.Name,
                    Image = userInfo.Image,
                    FriendsSince = friendship.AcceptedAt ?? friendship.CreatedAt
                });
            }
        }

        return friends.OrderBy(f => f.Name);
    }

    public async Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(string userId)
    {
        var pendingRequests = await _friendshipRepository.GetPendingRequestsReceivedAsync(userId);
        var requests = new List<FriendRequestDto>();

        foreach (var request in pendingRequests)
        {
            var senderInfo = await _userServiceClient.GetUserByIdAsync(request.SenderId);
            if (senderInfo != null)
            {
                requests.Add(new FriendRequestDto
                {
                    Id = request.Id,
                    SenderId = request.SenderId,
                    SenderName = senderInfo.Name,
                    SenderImage = senderInfo.Image,
                    CreatedAt = request.CreatedAt
                });
            }
        }

        return requests;
    }

    public async Task<IEnumerable<UserInfoDto>> GetAvailableUsersAsync(string userId)
    {
        // Get all users from UserService
        var allUsers = await _userServiceClient.GetAllUsersAsync();
        
        // Get existing friendships and pending requests for this user
        var existingFriendships = await _friendshipRepository.GetUserFriendsAsync(userId);
        var pendingRequestsSent = await _friendshipRepository.GetPendingRequestsSentAsync(userId);
        var pendingRequestsReceived = await _friendshipRepository.GetPendingRequestsReceivedAsync(userId);

        // Create a set of user IDs to exclude
        var excludedUserIds = new HashSet<string> { userId }; // Exclude the current user

        // Add friends
        foreach (var friendship in existingFriendships)
        {
            var friendId = friendship.SenderId == userId ? friendship.ReceiverId : friendship.SenderId;
            excludedUserIds.Add(friendId);
        }

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
        return allUsers.Where(user => !excludedUserIds.Contains(user.Id));
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
        var receiverInfo = await _userServiceClient.GetUserByIdAsync(receiverId);
        if (receiverInfo == null)
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

    public async Task RemoveFriendAsync(string userId1, string userId2)
    {
        var friendship = await _friendshipRepository.GetFriendshipAsync(userId1, userId2);
        if (friendship == null)
        {
            throw new ArgumentException("Friendship not found");
        }

        if (friendship.SenderId != userId1 && friendship.ReceiverId != userId1)
        {
            throw new UnauthorizedAccessException("You can only remove your own friendships");
        }

        await _friendshipRepository.DeleteAsync(friendship.Id);
    }

    public async Task<bool> AreFriendsAsync(string userId1, string userId2)
    {
        return await _friendshipRepository.AreFriendsAsync(userId1, userId2);
    }

    // New HttpContext-aware methods that handle all business logic
    public async Task<IEnumerable<FriendDto>> GetFriendsAsync(HttpContext httpContext)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("Getting friends for user {UserId}", currentUserId);
        return await GetUserFriendsAsync(currentUserId);
    }

    public async Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(HttpContext httpContext)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("Getting pending requests for user {UserId}", currentUserId);
        return await GetPendingRequestsAsync(currentUserId);
    }

    public async Task<IEnumerable<UserInfoDto>> GetAvailableUsersAsync(HttpContext httpContext)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("Getting available users for user {UserId}", currentUserId);
        return await GetAvailableUsersAsync(currentUserId);
    }

    public async Task<FriendshipDto> SendFriendRequestAsync(CreateFriendRequestDto request, HttpContext httpContext)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("User {UserId} sending friend request to {ReceiverId}", currentUserId, request.ReceiverId);
        return await SendFriendRequestAsync(currentUserId, request.ReceiverId);
    }

    public async Task<FriendshipDto> AcceptFriendRequestAsync(Guid friendshipId, HttpContext httpContext)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("User {UserId} accepting friend request {FriendshipId}", currentUserId, friendshipId);
        return await AcceptFriendRequestAsync(friendshipId, currentUserId);
    }

    public async Task<FriendshipDto> RejectFriendRequestAsync(Guid friendshipId, HttpContext httpContext)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("User {UserId} rejecting friend request {FriendshipId}", currentUserId, friendshipId);
        return await RejectFriendRequestAsync(friendshipId, currentUserId);
    }

    public async Task RemoveFriendAsync(string friendUserId, HttpContext httpContext)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("User {UserId} removing friend {FriendUserId}", currentUserId, friendUserId);
        await RemoveFriendAsync(currentUserId, friendUserId);
    }

    public async Task<bool> CheckFriendshipAsync(string userId, HttpContext httpContext)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        _logger.LogInformation("User {UserId} checking friendship with {OtherUserId}", currentUserId, userId);
        return await AreFriendsAsync(currentUserId, userId);
    }
} 