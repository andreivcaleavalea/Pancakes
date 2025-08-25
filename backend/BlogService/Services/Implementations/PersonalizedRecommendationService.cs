using BlogService.Models.Entities;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BlogService.Services.Implementations;

/// <summary>
/// Service wrapper that provides HTTP context-aware personalized recommendations
/// This bridges the gap between HTTP requests and the core recommendation engine
/// </summary>
public class PersonalizedRecommendationService
{
    private readonly RecommendationService _recommendationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PersonalizedRecommendationService> _logger;

    public PersonalizedRecommendationService(
        RecommendationService recommendationService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PersonalizedRecommendationService> logger)
    {
        _recommendationService = recommendationService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Get personalized recommendations with HTTP context awareness for auth tokens
    /// </summary>
    public async Task<IEnumerable<BlogPost>> GetPersonalizedRecommendationsWithContextAsync(string userId, int count = 5, string? excludeAuthorId = null)
    {
        try
        {
            // Try to get auth token from HTTP context for social signals
            var authToken = GetAuthTokenFromContext();
            
            if (!string.IsNullOrEmpty(authToken))
            {
                // Use real-time computation with social signals
                return await _recommendationService.ComputePersonalizedRecommendationsWithTokenAsync(userId, count, excludeAuthorId, authToken);
            }
            else
            {
                // Fall back to regular personalized recommendations (no social signals)
                return await _recommendationService.GetPersonalizedRecommendationsAsync(userId, count, excludeAuthorId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personalized recommendations with context for user {UserId}", userId);
            return await _recommendationService.GetPersonalizedRecommendationsAsync(userId, count, excludeAuthorId);
        }
    }

    private string? GetAuthTokenFromContext()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader.Substring("Bearer ".Length);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting auth token from HTTP context");
            return null;
        }
    }
}
