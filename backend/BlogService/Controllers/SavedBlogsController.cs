using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SavedBlogsController : ControllerBase
{
    private readonly ISavedBlogService _savedBlogService;

    public SavedBlogsController(ISavedBlogService savedBlogService)
    {
        _savedBlogService = savedBlogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSavedBlogs()
    {
        return await _savedBlogService.GetSavedBlogsAsync(HttpContext);
    }

    [HttpPost]
    public async Task<IActionResult> SaveBlog([FromBody] CreateSavedBlogDto createDto)
    {
        return await _savedBlogService.SaveBlogAsync(HttpContext, createDto, ModelState);
    }

    [HttpDelete("{blogPostId:guid}")]
    public async Task<IActionResult> UnsaveBlog(Guid blogPostId)
    {
        return await _savedBlogService.UnsaveBlogAsync(HttpContext, blogPostId);
    }

    [HttpGet("check/{blogPostId:guid}")]
    public async Task<IActionResult> IsBookmarked(Guid blogPostId)
    {
        return await _savedBlogService.IsBookmarkedAsync(HttpContext, blogPostId);
    }
}