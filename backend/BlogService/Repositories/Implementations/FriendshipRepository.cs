using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly BlogDbContext _context;

    public FriendshipRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Friendship?> GetByIdAsync(Guid id)
    {
        return await _context.Friendships.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Friendship?> GetFriendshipAsync(string senderId, string receiverId)
    {
        return await _context.Friendships
            .FirstOrDefaultAsync(f => 
                (f.SenderId == senderId && f.ReceiverId == receiverId) ||
                (f.SenderId == receiverId && f.ReceiverId == senderId));
    }

    public async Task<IEnumerable<Friendship>> GetUserFriendsAsync(string userId)
    {
        return await _context.Friendships
            .Where(f => (f.SenderId == userId || f.ReceiverId == userId) && 
                       f.Status == FriendshipStatus.Accepted)
            .OrderByDescending(f => f.AcceptedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Friendship>> GetPendingRequestsReceivedAsync(string userId)
    {
        return await _context.Friendships
            .Where(f => f.ReceiverId == userId && f.Status == FriendshipStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Friendship>> GetPendingRequestsSentAsync(string userId)
    {
        return await _context.Friendships
            .Where(f => f.SenderId == userId && f.Status == FriendshipStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllUsersExceptFriendsAsync(string userId)
    {
        // Get all user IDs that are already friends or have pending requests
        var existingRelationships = await _context.Friendships
            .Where(f => (f.SenderId == userId || f.ReceiverId == userId))
            .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
            .ToListAsync();

        // For now, we'll return an empty list and let the service handle fetching users
        // This will be expanded when we have access to all users
        return new List<string>();
    }

    public async Task<Friendship> CreateAsync(Friendship friendship)
    {
        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();
        return friendship;
    }

    public async Task<Friendship> UpdateAsync(Friendship friendship)
    {
        friendship.UpdatedAt = DateTime.UtcNow;
        if (friendship.Status == FriendshipStatus.Accepted && friendship.AcceptedAt == null)
        {
            friendship.AcceptedAt = DateTime.UtcNow;
        }
        _context.Friendships.Update(friendship);
        await _context.SaveChangesAsync();
        return friendship;
    }

    public async Task DeleteAsync(Guid id)
    {
        var friendship = await GetByIdAsync(id);
        if (friendship != null)
        {
            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> AreFriendsAsync(string userId1, string userId2)
    {
        return await _context.Friendships
            .AnyAsync(f => 
                ((f.SenderId == userId1 && f.ReceiverId == userId2) ||
                 (f.SenderId == userId2 && f.ReceiverId == userId1)) &&
                f.Status == FriendshipStatus.Accepted);
    }
} 