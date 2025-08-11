using UserService.Services.Interfaces;

namespace UserService.Services.Implementations.ProfilePictureStrategies;

/// <summary>
/// Factory for creating appropriate profile picture strategies based on provider type
/// </summary>
public class ProfilePictureStrategyFactory : IProfilePictureStrategyFactory
{
    private readonly IEnumerable<IProfilePictureStrategy> _strategies;
    private readonly ILogger<ProfilePictureStrategyFactory> _logger;

    public ProfilePictureStrategyFactory(
        IEnumerable<IProfilePictureStrategy> strategies,
        ILogger<ProfilePictureStrategyFactory> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }

    public IProfilePictureStrategy GetStrategy(string provider)
    {
        if (string.IsNullOrEmpty(provider))
        {
            _logger.LogWarning("Provider is null or empty, defaulting to OAuth strategy");
            return GetOAuthStrategy();
        }

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(provider));
        
        if (strategy == null)
        {
            _logger.LogWarning("No strategy found for provider: {Provider}, defaulting to OAuth strategy", provider);
            return GetOAuthStrategy();
        }

        _logger.LogDebug("Using strategy {StrategyType} for provider {Provider}", 
            strategy.GetType().Name, provider);
        
        return strategy;
    }

    public IProfilePictureStrategy GetStrategyForUserUpload()
    {
        var strategy = _strategies.OfType<SelfProvidedProfilePictureStrategy>().FirstOrDefault();
        
        if (strategy == null)
        {
            throw new InvalidOperationException(
                "SelfProvidedProfilePictureStrategy not found. Ensure it's registered in DI container.");
        }

        return strategy;
    }

    private IProfilePictureStrategy GetOAuthStrategy()
    {
        var strategy = _strategies.OfType<OAuthProfilePictureStrategy>().FirstOrDefault();
        
        if (strategy == null)
        {
            throw new InvalidOperationException(
                "OAuthProfilePictureStrategy not found. Ensure it's registered in DI container.");
        }

        return strategy;
    }
}