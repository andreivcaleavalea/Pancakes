using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TestMicroservice.Services;

namespace TestMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints in this controller
    public class UserDataController : ControllerBase
    {
        private readonly CurrentUserService _currentUserService;

        public UserDataController(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Example endpoint showing how to get user-specific data in a stateless way.
        /// The user ID comes from the JWT token, not from storage.
        /// </summary>
        [HttpGet("profile")]
        public IActionResult GetUserProfile()
        {
            try
            {
                var currentUser = _currentUserService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // In a real application, you might query a database using the user ID
                // Example: var userProfile = await _profileService.GetProfileAsync(currentUser.Id);
                
                var userProfile = new
                {
                    UserId = currentUser.Id,
                    Name = currentUser.Name,
                    Email = currentUser.Email,
                    Image = currentUser.Image,
                    Provider = currentUser.Provider,
                    MemberSince = currentUser.CreatedAt,
                    LastActive = currentUser.LastLoginAt,
                    // Add any additional profile data here
                    Preferences = new
                    {
                        Theme = "light",
                        Language = "en"
                    }
                };

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user profile: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Example endpoint for user-specific actions.
        /// Shows how to work with the authenticated user in a stateless system.
        /// </summary>
        [HttpPost("favorites/{itemId}")]
        public IActionResult AddToFavorites(string itemId)
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // In a real application, you would save to a database using the user ID
                // Example: await _favoritesService.AddFavoriteAsync(userId, itemId);
                
                Console.WriteLine($"User {userId} added item {itemId} to favorites");
                
                return Ok(new { 
                    message = "Item added to favorites",
                    userId = userId,
                    itemId = itemId,
                    addedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to favorites: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Example endpoint showing how to return user-specific data.
        /// The data filtering is based on the user ID from the JWT token.
        /// </summary>
        [HttpGet("activities")]
        public IActionResult GetUserActivities()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // In a real application, you would query user-specific data from a database
                // Example: var activities = await _activityService.GetUserActivitiesAsync(userId);
                
                var activities = new[]
                {
                    new { Id = 1, Action = "Login", Timestamp = DateTime.UtcNow.AddMinutes(-30), UserId = userId },
                    new { Id = 2, Action = "View Profile", Timestamp = DateTime.UtcNow.AddMinutes(-15), UserId = userId },
                    new { Id = 3, Action = "Update Settings", Timestamp = DateTime.UtcNow.AddMinutes(-5), UserId = userId }
                };

                return Ok(new
                {
                    UserId = userId,
                    Activities = activities,
                    TotalCount = activities.Length
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user activities: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
