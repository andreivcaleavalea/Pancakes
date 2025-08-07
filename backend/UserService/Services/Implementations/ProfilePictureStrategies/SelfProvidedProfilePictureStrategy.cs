using UserService.Models.Authentication;
using UserService.Models.Entities;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations.ProfilePictureStrategies;

/// <summary>
/// Strategy for handling user-uploaded profile pictures
/// Once a user uploads their own picture, it should never be overridden by OAuth
/// </summary>
public class SelfProvidedProfilePictureStrategy : IProfilePictureStrategy
{
    private readonly ILogger<SelfProvidedProfilePictureStrategy> _logger;
    private readonly IFileService _fileService;
    private const string SelfProvidedProviderValue = "selfprovided";

    public SelfProvidedProfilePictureStrategy(
        ILogger<SelfProvidedProfilePictureStrategy> logger,
        IFileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    public bool CanHandle(string provider)
    {
        return string.Equals(provider, SelfProvidedProviderValue, StringComparison.OrdinalIgnoreCase);
    }

    public bool ShouldUpdatePictureFromOAuth(User user, OAuthUserInfo oauthInfo, string provider)
    {
        // Never update picture from OAuth if user has uploaded their own
        _logger.LogInformation(
            "Preserving user-uploaded profile picture for user {UserId}. " +
            "OAuth update from {Provider} was ignored to protect user's custom image.",
            user.Id, provider);
        return false;
    }

    public void HandleUserUpload(User user, string imagePath)
    {
        // Delete old profile picture if it exists and is a local file
        if (!string.IsNullOrEmpty(user.Image) && 
            user.Image.StartsWith("assets/profile-pictures/", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                _fileService.DeleteProfilePictureAsync(user.Image).Wait();
                _logger.LogInformation("Deleted old profile picture: {OldImage}", user.Image);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old profile picture: {OldImage}", user.Image);
            }
        }

        // Update user with new image and change provider to selfprovided
        user.Image = imagePath;
        user.Provider = SelfProvidedProviderValue;
        user.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "User {UserId} uploaded profile picture. Provider changed to {Provider}, Image: {Image}",
            user.Id, SelfProvidedProviderValue, imagePath);
    }

    public string GetProviderValue()
    {
        return SelfProvidedProviderValue;
    }
}