using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        return await _userService.GetByIdAsync(HttpContext, id);
    }

    [HttpGet("email/{email}")]
    [Authorize]
    public async Task<IActionResult> GetByEmail(string email)
    {
        return await _userService.GetByEmailAsync(HttpContext, email);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
    {
        if (!string.IsNullOrEmpty(search))
        {
            return await _userService.SearchUsersAsync(HttpContext, search, page, pageSize);
        }
        return await _userService.GetAllAsync(HttpContext, page, pageSize);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createDto)
    {
        return await _userService.CreateAsync(HttpContext, createDto, ModelState);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto updateDto)
    {
        return await _userService.UpdateAsync(HttpContext, id, updateDto, ModelState);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        return await _userService.DeleteAsync(HttpContext, id);
    }

    [HttpPost("{id}/ban")]
    [Authorize]
    public async Task<IActionResult> BanUser(string id, [FromBody] BanUserRequest request)
    {
        request.UserId = id; // Ensure the ID from route is used
        return await _userService.BanUserAsync(HttpContext, request);
    }

    [HttpPost("{id}/unban")]
    [Authorize]
    public async Task<IActionResult> UnbanUser(string id, [FromBody] UnbanUserRequest request)
    {
        request.UserId = id; // Ensure the ID from route is used
        return await _userService.UnbanUserAsync(HttpContext, request);
    }
}
