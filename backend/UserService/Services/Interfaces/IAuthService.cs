using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.Requests;

namespace UserService.Services.Interfaces;

public interface IAuthService
{
    Task<IActionResult> LoginAsync(HttpContext httpContext, LoginRequest request);
    IActionResult OAuthCallback(string provider, string code, string state);
    IActionResult Logout(HttpContext httpContext);
    IActionResult GetCurrentUser(HttpContext httpContext);
    IActionResult ValidateToken(HttpContext httpContext);
}
