using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class FriendsPostService : IFriendsPostService
{
    private readonly IFriendshipService _friendshipService;
    private readonly IBlogPostService _blogPostService;
    private readonly IJwtUserService _jwtUserService;
    private readonly ILogger<FriendsPostService> _logger;

    public FriendsPostService(
        IFriendshipService friendshipService, 
        IBlogPostService blogPostService,
        IJwtUserService jwtUserService,
        ILogger<FriendsPostService> logger)
    {
        _friendshipService = friendshipService;
        _blogPostService = blogPostService;
        _jwtUserService = jwtUserService;
        _logger = logger;
    }

    public async Task<object> GetFriendsPostsAsync(string currentUserId, int page, int pageSize)
    {
        _logger.LogInformation("Getting friends' posts for user {UserId}, page {Page}, pageSize {PageSize}", 
            currentUserId, page, pageSize);

        // Get friends list
        var friends = await _friendshipService.GetUserFriendsAsync(currentUserId);
        var friendUserIds = friends.Select(f => f.UserId).ToList();

        if (!friendUserIds.Any())
        {
            _logger.LogInformation("User {UserId} has no friends, returning empty result", currentUserId);
            // Return empty result if user has no friends
            return new { posts = new List<BlogPostDto>(), totalCount = 0, page, pageSize };
        }

        _logger.LogInformation("Found {FriendCount} friends for user {UserId}", friendUserIds.Count, currentUserId);
        
        var result = await _blogPostService.GetFriendsPostsAsync(friendUserIds, page, pageSize);
        return result;
    }

    public async Task<object> GetFriendsPostsAsync(HttpContext httpContext, int page, int pageSize)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return await GetFriendsPostsAsync(currentUserId, page, pageSize);
    }
}
