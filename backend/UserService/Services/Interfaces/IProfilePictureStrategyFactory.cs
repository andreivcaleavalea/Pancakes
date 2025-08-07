namespace UserService.Services.Interfaces;

/// <summary>
/// Factory interface for creating profile picture strategies
/// </summary>
public interface IProfilePictureStrategyFactory
{
    /// <summary>
    /// Gets the appropriate strategy based on the current provider
    /// </summary>
    /// <param name="provider">The current provider value from the user</param>
    /// <returns>The strategy to handle profile picture operations</returns>
    IProfilePictureStrategy GetStrategy(string provider);
    
    /// <summary>
    /// Gets the strategy specifically for handling user uploads
    /// </summary>
    /// <returns>The strategy for user-uploaded pictures</returns>
    IProfilePictureStrategy GetStrategyForUserUpload();
}