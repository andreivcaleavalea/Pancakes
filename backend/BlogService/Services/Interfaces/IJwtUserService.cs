namespace BlogService.Services.Interfaces
{
    /// <summary>
    /// Interface for extracting user information from JWT tokens
    /// </summary>
    public interface IJwtUserService
    {
        /// <summary>
        /// Gets the current user's ID from the JWT token in the authorization header.
        /// </summary>
        /// <returns>User ID if authenticated, null otherwise</returns>
        string? GetCurrentUserId();

        /// <summary>
        /// Checks if the current request has a valid JWT token.
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        bool IsAuthenticated();
    }
}
