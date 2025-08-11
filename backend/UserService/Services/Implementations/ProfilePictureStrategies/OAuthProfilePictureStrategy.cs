using UserService.Models.Authentication;
using UserService.Models.Entities;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations.ProfilePictureStrategies;

/// <summary>
/// Strategy for handling profile pictures from OAuth providers (Google, GitHub)
/// </summary>
public class OAuthProfilePictureStrategy : IProfilePictureStrategy
{
    private readonly ILogger<OAuthProfilePictureStrategy> _logger;
    private readonly HashSet<string> _supportedProviders = new() { "google", "github" };

    public OAuthProfilePictureStrategy(ILogger<OAuthProfilePictureStrategy> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(string provider)
    {
        return _supportedProviders.Contains(provider?.ToLowerInvariant() ?? string.Empty);
    }

    public bool ShouldUpdatePictureFromOAuth(User user, OAuthUserInfo oauthInfo, string provider)
    {
        // Only update picture if the user is still using OAuth provider
        if (!CanHandle(user.Provider))
        {
            _logger.LogInformation(
                "Preserving user-uploaded profile picture for user {UserId}. " +
                "Current provider: {CurrentProvider}, OAuth provider: {OAuthProvider}",
                user.Id, user.Provider, provider);
            return false;
        }

        // Update the picture from OAuth
        user.Image = oauthInfo.Picture;
        _logger.LogInformation(
            "Updated profile picture from {Provider} OAuth for user {UserId}",
            provider, user.Id);
        return true;
    }

    public void HandleUserUpload(User user, string imagePath)
    {
        // This strategy doesn't handle user uploads
        throw new InvalidOperationException(
            "OAuth strategy cannot handle user uploads. Use SelfProvidedProfilePictureStrategy instead.");
    }

    public string GetProviderValue()
    {
        // This method is not used for OAuth strategy as it preserves the original provider
        throw new InvalidOperationException(
            "OAuth strategy preserves the original provider value (google, github, etc.)");
    }
}