namespace BlogService.Services.Interfaces;

public interface IUserContextService
{
    string GetCurrentUserId(HttpContext? httpContext = null);
    string GetAnonymousUserId(HttpContext httpContext);
}
