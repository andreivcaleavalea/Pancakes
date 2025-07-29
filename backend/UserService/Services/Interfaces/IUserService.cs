using UserService.Models.Authentication;
using UserService.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UserService.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(string id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByProviderAndProviderUserIdAsync(string provider, string providerUserId);
    Task<IEnumerable<UserDto>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<UserDto> CreateAsync(CreateUserDto createDto);
    Task<UserDto> UpdateAsync(string id, UpdateUserDto updateDto);
    Task DeleteAsync(string id);
    Task<UserDto> CreateOrUpdateFromOAuthAsync(OAuthUserInfo oauthInfo, string provider);

    // HttpContext-aware methods for controller use
    Task<IActionResult> GetByIdAsync(HttpContext httpContext, string id);
    Task<IActionResult> GetByEmailAsync(HttpContext httpContext, string email);
    Task<IActionResult> GetAllAsync(HttpContext httpContext, int page, int pageSize);
    Task<IActionResult> CreateAsync(HttpContext httpContext, CreateUserDto createDto, ModelStateDictionary modelState);
    Task<IActionResult> UpdateAsync(HttpContext httpContext, string id, UpdateUserDto updateDto, ModelStateDictionary modelState);
    Task<IActionResult> DeleteAsync(HttpContext httpContext, string id);
}
