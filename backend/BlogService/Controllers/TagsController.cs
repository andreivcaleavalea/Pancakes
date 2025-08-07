using Microsoft.AspNetCore.Mvc;
using BlogService.Services.Interfaces;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagService tagService, ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularTags([FromQuery] int limit = 20)
    {
        try
        {
            var tags = await _tagService.GetPopularTagsAsync(limit);
            return Ok(tags);
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
            var tags = await _tagService.SearchTagsAsync(query, limit);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tags with query: {Query}", query);
            return StatusCode(500, "An error occurred while searching tags");
        }
    }
}