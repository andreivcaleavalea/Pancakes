using UserService.Models;

namespace UserService.Services
{
    public class UserManagementService
    {
        /// <summary>
        /// Creates a new user from OAuth information.
        /// This is stateless - no persistence, returns a user object ready for JWT token generation.
        /// </summary>
        /// <param name="oauthInfo">OAuth user information</param>
        /// <param name="provider">OAuth provider name</param>
        /// <returns>New user object</returns>
        public User CreateUserFromOAuth(OAuthUserInfo oauthInfo, string provider)
        {
            var user = new User
            {
                Id = GenerateUniqueUserId(provider, oauthInfo.Id),
                Name = oauthInfo.Name,
                Email = oauthInfo.Email,
                Image = oauthInfo.Picture,
                Provider = provider,
                ProviderUserId = oauthInfo.Id,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            Console.WriteLine($"Created user from OAuth: {user.Name} ({user.Email}) via {provider}");
            return user;
        }

        /// <summary>
        /// Generates a unique user ID based on provider and provider user ID.
        /// This ensures the same user from the same provider always gets the same ID.
        /// </summary>
        /// <param name="provider">OAuth provider</param>
        /// <param name="providerUserId">Provider-specific user ID</param>
        /// <returns>Deterministic unique user ID</returns>
        private string GenerateUniqueUserId(string provider, string providerUserId)
        {
            // Create a deterministic ID based on provider and provider user ID
            // This ensures the same user from the same provider always gets the same ID
            var combinedString = $"{provider}:{providerUserId}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedString));
            return Convert.ToHexString(hashBytes)[..32]; // Take first 32 characters for a reasonable ID length
        }

        /// <summary>
        /// Validates if a user ID matches the expected format and provider pattern.
        /// </summary>
        /// <param name="userId">User ID to validate</param>
        /// <param name="provider">Expected provider</param>
        /// <param name="providerUserId">Expected provider user ID</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateUserId(string userId, string provider, string providerUserId)
        {
            var expectedId = GenerateUniqueUserId(provider, providerUserId);
            return userId.Equals(expectedId, StringComparison.OrdinalIgnoreCase);
        }
    }
}