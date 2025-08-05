using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminService.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static string? GetCurrentAdminId(this ControllerBase controller)
        {
            return controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetClientIpAddress(this ControllerBase controller)
        {
            return controller.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        public static string GetUserAgent(this ControllerBase controller)
        {
            return controller.HttpContext.Request.Headers.UserAgent.ToString();
        }
    }
}
