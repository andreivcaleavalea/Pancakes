using Microsoft.AspNetCore.Mvc;
using BlogService.Repositories.Interfaces;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly ILogger<TagsController> _logger;

    public TagsController(IBlogPostRepository blogPostRepository, ILogger<TagsController> logger)
    {
        _blogPostRepository = blogPostRepository;
        _logger = logger;
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularTags([FromQuery] int limit = 20)
    {
        try
        {
            var (posts, _) = await _blogPostRepository.GetAllAsync(new Models.Requests.BlogPostQueryParameters 
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

            return Ok(tagCounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular tags");
            return StatusCode(500, "An error occurred while getting popular tags");
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchTags([FromQuery] string query, [FromQuery] int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Ok(new List<string>());
            }

            var (posts, _) = await _blogPostRepository.GetAllAsync(new Models.Requests.BlogPostQueryParameters 
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

            return Ok(matchingTags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tags with query: {Query}", query);
            return StatusCode(500, "An error occurred while searching tags");
        }
    }
}