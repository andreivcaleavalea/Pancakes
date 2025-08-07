using UserService.Models.Authentication;
using UserService.Models.Entities;

namespace UserService.Services.Interfaces;

/// <summary>
/// Strategy interface for managing profile pictures based on provider type
/// </summary>
public interface IProfilePictureStrategy
{
    /// <summary>
    /// Determines if this strategy can handle the given provider
    /// </summary>
    /// <param name="provider">The provider string from the user entity</param>
    /// <returns>True if this strategy handles the provider</returns>
    bool CanHandle(string provider);
    
    /// <summary>
    /// Updates the user's profile picture information during OAuth login
    /// </summary>
    /// <param name="user">The user entity to update</param>
    /// <param name="oauthInfo">OAuth information containing picture URL</param>
    /// <param name="provider">The OAuth provider (google, github, etc.)</param>
    /// <returns>True if the picture was updated, false if it should be preserved</returns>
    bool ShouldUpdatePictureFromOAuth(User user, OAuthUserInfo oauthInfo, string provider);
    
    /// <summary>
    /// Handles the profile picture update when a user uploads their own picture
    /// </summary>
    /// <param name="user">The user entity to update</param>
    /// <param name="imagePath">The path to the uploaded image</param>
    void HandleUserUpload(User user, string imagePath);
    
    /// <summary>
    /// Gets the appropriate provider value for this strategy
    /// </summary>
    /// <returns>The provider string that should be stored in the database</returns>
    string GetProviderValue();
}