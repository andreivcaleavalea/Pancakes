using TestMicroservice.Models;
using System.Text.Json;

namespace TestMicroservice.Services
{
    public class UserService
    {
        private readonly string _filePath = "users.json";
        private readonly object _lock = new object();

        public User? GetUserByProviderAndId(string provider, string providerUserId)
        {
            var users = LoadUsers();
            return users.FirstOrDefault(u => u.Provider == provider && u.ProviderUserId == providerUserId);
        }

        public User CreateOrUpdateUser(OAuthUserInfo oauthInfo, string provider)
        {
            lock (_lock)
            {
                var users = LoadUsers();
                var existingUser = users.FirstOrDefault(u => 
                    u.Provider == provider && u.ProviderUserId == oauthInfo.Id);

                if (existingUser != null)
                {
                    // Update existing user
                    existingUser.Name = oauthInfo.Name;
                    existingUser.Email = oauthInfo.Email;
                    existingUser.Image = oauthInfo.Picture;
                    existingUser.LastLoginAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new user
                    existingUser = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = oauthInfo.Name,
                        Email = oauthInfo.Email,
                        Image = oauthInfo.Picture,
                        Provider = provider,
                        ProviderUserId = oauthInfo.Id,
                        CreatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };
                    users.Add(existingUser);
                }

                SaveUsers(users);
                return existingUser;
            }
        }

        private List<User> LoadUsers()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<User>();

                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                return new List<User>();
            }
        }

        private void SaveUsers(List<User> users)
        {
            try
            {
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users: {ex.Message}");
            }
        }
    }
}