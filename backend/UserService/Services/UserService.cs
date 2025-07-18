using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;
using UserService.Models.Entities;

namespace UserService.Services
{
    public class UserManagementService
    {
        private readonly UserDbContext _context;

        public UserManagementService(UserDbContext context)
        {
            _context = context;
        }
        
        public async Task<Models.User> CreateOrUpdateUserFromOAuthAsync(OAuthUserInfo oauthInfo, string provider)
        {
            var userId = GenerateUniqueUserId(provider, oauthInfo.Id);
            
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderUserId == oauthInfo.Id);

            if (existingUser != null)
            {
                // Update existing user's last login and potentially other info
                existingUser.LastLoginAt = DateTime.UtcNow;
                existingUser.Name = oauthInfo.Name; // Update name in case it changed
                existingUser.Image = oauthInfo.Picture; // Update profile picture
                existingUser.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"Updated existing user: {existingUser.Name} ({existingUser.Email}) via {provider}");
                
                return MapToLegacyUser(existingUser);
            }

            // Create new user
            var newUser = new Models.Entities.User
            {
                Id = userId,
                Name = oauthInfo.Name,
                Email = oauthInfo.Email,
                Image = oauthInfo.Picture,
                Provider = provider,
                ProviderUserId = oauthInfo.Id,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Created new user: {newUser.Name} ({newUser.Email}) via {provider}");
            return MapToLegacyUser(newUser);
        }
        
        public async Task<Models.User?> GetUserByIdAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null ? MapToLegacyUser(user) : null;
        }
        
        private Models.User MapToLegacyUser(Models.Entities.User entityUser)
        {
            return new Models.User
            {
                Id = entityUser.Id,
                Name = entityUser.Name,
                Email = entityUser.Email,
                Image = entityUser.Image,
                Provider = entityUser.Provider,
                ProviderUserId = entityUser.ProviderUserId,
                Bio = entityUser.Bio,
                PhoneNumber = entityUser.PhoneNumber,
                DateOfBirth = entityUser.DateOfBirth,
                CreatedAt = entityUser.CreatedAt,
                LastLoginAt = entityUser.LastLoginAt,
                UpdatedAt = entityUser.UpdatedAt
            };
        }

        private string GenerateUniqueUserId(string provider, string providerUserId)
        {
            // Create a deterministic ID based on provider and provider user ID
            // This ensures the same user from the same provider always gets the same ID
            var combinedString = $"{provider}:{providerUserId}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedString));
            return Convert.ToHexString(hashBytes)[..32]; // Take first 32 characters for a reasonable ID length
        }
        
        public bool ValidateUserId(string userId, string provider, string providerUserId)
        {
            var expectedId = GenerateUniqueUserId(provider, providerUserId);
            return userId.Equals(expectedId, StringComparison.OrdinalIgnoreCase);
        }
    }
}