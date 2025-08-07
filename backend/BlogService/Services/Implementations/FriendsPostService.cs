using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using BlogService.Helpers;

namespace BlogService.Services.Implementations;

public class FriendsPostService : IFriendsPostService
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IBlogPostService _blogPostService;
    private readonly IJwtUserService _jwtUserService;
    private readonly ILogger<FriendsPostService> _logger;

    public FriendsPostService(
        IUserServiceClient userServiceClient, 
        IBlogPostService blogPostService,
        IJwtUserService jwtUserService,
        ILogger<FriendsPostService> logger)
    {
        _userServiceClient = userServiceClient;
        _blogPostService = blogPostService;
        _jwtUserService = jwtUserService;
        _logger = logger;
    }



    public async Task<object> GetFriendsPostsAsync(HttpContext httpContext, int page, int pageSize)
    {
        var currentUserId = _jwtUserService.GetCurrentUserId();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // Get the auth token from the request
        var authToken = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            throw new UnauthorizedAccessException("No authentication token provided");
        }

        _logger.LogInformation("Getting friends' posts for user {UserId}, page {Page}, pageSize {PageSize}", 
            currentUserId, page, pageSize);

        // Get friends list from UserService
        var friends = await _userServiceClient.GetUserFriendsAsync(authToken);
        var friendUserIds = friends.Select(f => f.UserId).ToList();

        if (!friendUserIds.Any())
        {
            _logger.LogWarning("User {UserId} has no friends, returning empty result", currentUserId);
            // Return empty result if user has no friends
            return new { data = new List<BlogPostDto>(), pagination = new { currentPage = page, totalPages = 0, totalItems = 0, pageSize = pageSize } };
        }

        _logger.LogInformation("Found {FriendCount} friends for user {UserId}: [{FriendIds}]", 
            friendUserIds.Count, currentUserId, string.Join(", ", friendUserIds));
        
        var result = await _blogPostService.GetFriendsPostsAsync(friendUserIds, page, pageSize);
        _logger.LogInformation("Retrieved {PostCount} posts from friends for user {UserId}", 
            (result as PaginatedResult<BlogPostDto>)?.Data?.Count() ?? 0, currentUserId);
        
        return result;
    }
}
