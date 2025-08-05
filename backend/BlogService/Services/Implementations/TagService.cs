using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class TagService : ITagService
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly ILogger<TagService> _logger;

    public TagService(IBlogPostRepository blogPostRepository, ILogger<TagService> logger)
    {
        _blogPostRepository = blogPostRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetPopularTagsAsync(int limit = 20)
    {
        try
        {
            var (posts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = 1000, // Get a large sample
                Page = 1 
            });

            // Extract all tags and count their usage
            var tagCounts = posts
                .SelectMany(p => p.Tags ?? new List<string>())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .GroupBy(tag => tag.ToLower().Trim())
                .Select(g => new { Tag = g.First(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .Select(x => x.Tag)
                .ToList();

            _logger.LogInformation("Retrieved {TagCount} popular tags", tagCounts.Count);
            return tagCounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular tags");
            throw;
        }
    }

    public async Task<IEnumerable<string>> SearchTagsAsync(string query, int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                _logger.LogDebug("Search query is too short or empty: '{Query}'", query);
                return new List<string>();
            }

            var (posts, _) = await _blogPostRepository.GetAllAsync(new BlogPostQueryParameters 
            { 
                PageSize = 1000, // Get a large sample
                Page = 1 
            });

            // Search for tags that start with or contain the query
            var matchingTags = posts
                .SelectMany(p => p.Tags ?? new List<string>())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .Where(tag => tag.ToLower().Contains(query.ToLower().Trim()))
                .OrderBy(tag => tag.ToLower().StartsWith(query.ToLower().Trim()) ? 0 : 1) // Prioritize starts-with matches
                .ThenBy(tag => tag.Length) // Then by length
                .Take(limit)
                .ToList();

            _logger.LogInformation("Found {MatchCount} tags matching query '{Query}'", matchingTags.Count, query);
            return matchingTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tags with query: {Query}", query);
            throw;
        }
    }
}