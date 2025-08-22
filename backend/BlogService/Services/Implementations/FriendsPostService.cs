using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using BlogService.Helpers;

namespace BlogService.Services.Implementations;

public class FriendsPostService : IFriendsPostService
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IBlogPostService _blogPostService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<FriendsPostService> _logger;

    public FriendsPostService(
        IUserServiceClient userServiceClient, 
        IBlogPostService blogPostService,
        IAuthorizationService authorizationService,
        ILogger<FriendsPostService> logger)
    {
        _userServiceClient = userServiceClient;
        _blogPostService = blogPostService;
        _authorizationService = authorizationService;
        _logger = logger;
    }



    public async Task<object> GetFriendsPostsAsync(HttpContext httpContext, int page, int pageSize)
    {
        // Use the same authentication pattern as working endpoints (comments, drafts, etc.)
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext);
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Authorization token is required or invalid");
        }

        // Get the auth token from the request
        var authToken = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            throw new UnauthorizedAccessException("No authentication token provided");
        }

        _logger.LogInformation("Getting friends' posts for user {UserId} ({UserName}), page {Page}, pageSize {PageSize}", 
            currentUser.Id, currentUser.Name, page, pageSize);

        // Get friends list from UserService
        var friends = await _userServiceClient.GetUserFriendsAsync(authToken);
        var friendUserIds = friends.Select(f => f.UserId).ToList();

        if (!friendUserIds.Any())
        {
            _logger.LogWarning("User {UserId} ({UserName}) has no friends, returning empty result", currentUser.Id, currentUser.Name);
            // Return empty result if user has no friends
            return new { data = new List<BlogPostDto>(), pagination = new { currentPage = page, totalPages = 0, totalItems = 0, pageSize = pageSize } };
        }

        var result = await _blogPostService.GetFriendsPostsAsync(friendUserIds, page, pageSize);        
        return result;
    }
}
