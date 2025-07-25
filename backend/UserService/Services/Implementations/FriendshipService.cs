using AutoMapper;
using UserService.Models;
using UserService.Models.DTOs;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class FriendshipService : IFriendshipService
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public FriendshipService(
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _mapper = mapper;
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
} 