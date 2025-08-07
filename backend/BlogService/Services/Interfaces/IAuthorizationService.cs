using BlogService.Models.DTOs;

namespace BlogService.Services.Interfaces;

public interface IAuthorizationService
{
    Task<UserInfoDto?> GetCurrentUserAsync(HttpContext httpContext);
    string ExtractTokenFromHeader(HttpContext httpContext);
}
