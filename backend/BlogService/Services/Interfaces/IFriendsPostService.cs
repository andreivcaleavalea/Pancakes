using BlogService.Models.DTOs;
using BlogService.Helpers;

namespace BlogService.Services.Interfaces;

public interface IFriendsPostService
{
    Task<object> GetFriendsPostsAsync(HttpContext httpContext, int page, int pageSize);
}
