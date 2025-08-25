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

    public async Task<IEnumerable<string>> GetUserFriendsAsync(string userId)
    {
        // Get accepted friendships where user is either sender or receiver
        var friendships = await _context.Friendships
            .Where(f => (f.SenderId == userId || f.ReceiverId == userId) 
                       && f.Status == FriendshipStatus.Accepted)
            .ToListAsync();

        // Extract friend IDs (the other person in each friendship)
        var friendIds = friendships.Select(f => 
            f.SenderId == userId ? f.ReceiverId : f.SenderId)
            .Distinct()
            .ToList();

        return friendIds;
    }
}
