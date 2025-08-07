using AutoMapper;
using UserService.Models.Entities;
using UserService.Models.Authentication;
using UserService.Models.DTOs;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace UserService.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBanService _banService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IProfilePictureStrategyFactory _profilePictureStrategyFactory;

    public UserService(
        IUserRepository userRepository,
        IBanService banService,
        IMapper mapper,
        ILogger<UserService> logger,
        IProfilePictureStrategyFactory profilePictureStrategyFactory)
    {
        _userRepository = userRepository;
        _banService = banService;
        _mapper = mapper;
        _logger = logger;
        _profilePictureStrategyFactory = profilePictureStrategyFactory;
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto?> GetByProviderAndProviderUserIdAsync(string provider, string providerUserId)
    {
        var user = await _userRepository.GetByProviderAndProviderUserIdAsync(provider, providerUserId);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var users = await _userRepository.GetAllAsync(page, pageSize);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<IEnumerable<UserDto>> GetUsersByIdsAsync(IEnumerable<string> userIds)
    {
        if (userIds == null || !userIds.Any())
            return new List<UserDto>();

        var users = await _userRepository.GetUsersByIdsAsync(userIds);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<(IEnumerable<UserDto> users, int totalCount)> GetAllWithCountAsync(int page = 1, int pageSize = 10)
    {
        var (users, totalCount) = await _userRepository.GetAllWithCountAsync(page, pageSize);
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        return (userDtos, totalCount);
    }

    public async Task<(IEnumerable<UserDto> users, int totalCount)> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var (users, totalCount) = await _userRepository.SearchUsersAsync(searchTerm, page, pageSize);
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        return (userDtos, totalCount);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createDto)
    {
        // Check if user already exists by email
        if (await _userRepository.ExistsByEmailAsync(createDto.Email))
        {
            throw new ArgumentException($"User with email {createDto.Email} already exists.");
        }

        // Check if user already exists by provider and provider user ID
        if (await _userRepository.ExistsByProviderAndProviderUserIdAsync(createDto.Provider, createDto.ProviderUserId))
        {
            throw new ArgumentException($"User with provider {createDto.Provider} and provider user ID {createDto.ProviderUserId} already exists.");
        }

        var user = _mapper.Map<User>(createDto);
        user.Id = GenerateUniqueUserId(createDto.Provider, createDto.ProviderUserId);
        
        var createdUser = await _userRepository.CreateAsync(user);
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto> UpdateAsync(string id, UpdateUserDto updateDto)
    {
        var existingUser = await GetUserByIdOrThrowAsync(id);
        
        // If email is being updated, check if it's already in use
        if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != existingUser.Email)
        {
            if (await _userRepository.ExistsByEmailAsync(updateDto.Email))
            {
                throw new ArgumentException($"Email {updateDto.Email} is already in use by another user.");
            }
        }

        _mapper.Map(updateDto, existingUser);
        var updatedUser = await _userRepository.UpdateAsync(existingUser);
        return _mapper.Map<UserDto>(updatedUser);
    }

    public async Task DeleteAsync(string id)
    {
        await GetUserByIdOrThrowAsync(id);
        await _userRepository.DeleteAsync(id);
    }

    public async Task<UserDto> CreateOrUpdateFromOAuthAsync(OAuthUserInfo oauthInfo, string provider)
    {
        // First, check if user exists by provider and provider user ID (exact match)
        var existingUser = await _userRepository.GetByProviderAndProviderUserIdAsync(provider, oauthInfo.Id);
        
        if (existingUser != null)
        {
            // Update existing user with latest OAuth info and last login
            existingUser.Name = oauthInfo.Name;
            existingUser.Email = oauthInfo.Email;
            existingUser.LastLoginAt = DateTime.UtcNow;
            
            // Use strategy pattern to determine if profile picture should be updated
            var strategy = _profilePictureStrategyFactory.GetStrategy(existingUser.Provider);
            strategy.ShouldUpdatePictureFromOAuth(existingUser, oauthInfo, provider);
            
            var updatedUser = await _userRepository.UpdateAsync(existingUser);
            return _mapper.Map<UserDto>(updatedUser);
        }
        
        // If no user found by provider, check if a user exists with this email from another provider
        var existingUserByEmail = await _userRepository.GetByEmailAsync(oauthInfo.Email);
        
        if (existingUserByEmail != null)
        {
            // User exists with same email but different provider - link the accounts
            // Update the existing user to add this provider info and login
            existingUserByEmail.Name = oauthInfo.Name; // Update name in case it changed
            existingUserByEmail.LastLoginAt = DateTime.UtcNow;
            // Note: We keep the original Provider and ProviderUserId but allow login from multiple providers
            
            // Use strategy pattern to determine if profile picture should be updated
            var strategy = _profilePictureStrategyFactory.GetStrategy(existingUserByEmail.Provider);
            strategy.ShouldUpdatePictureFromOAuth(existingUserByEmail, oauthInfo, provider);
            
            var updatedUser = await _userRepository.UpdateAsync(existingUserByEmail);
            return _mapper.Map<UserDto>(updatedUser);
        }
        else
        {
            // Create new user from OAuth info
            var userId = GenerateUniqueUserId(provider, oauthInfo.Id);
            
            var newUser = new User
            {
                Id = userId,
                Name = oauthInfo.Name,
                Email = oauthInfo.Email,
                Image = oauthInfo.Picture,
                Provider = provider,
                ProviderUserId = oauthInfo.Id,
                Bio = string.Empty,
                PhoneNumber = string.Empty,
                DateOfBirth = null,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var createdUser = await _userRepository.CreateAsync(newUser);
            return _mapper.Map<UserDto>(createdUser);
        }
    }

    // UserManagement methods (merged from UserManagementService)
    /// <summary>
    /// Creates a new user from OAuth information.
    /// This is stateless - no persistence, returns a user object ready for JWT token generation.
    /// </summary>
    /// <param name="oauthInfo">OAuth user information</param>
    /// <param name="provider">OAuth provider name</param>
    /// <returns>New user object</returns>
    public User CreateUserFromOAuth(OAuthUserInfo oauthInfo, string provider)
    {
        var user = new User
        {
            Id = GenerateUniqueUserId(provider, oauthInfo.Id),
            Name = oauthInfo.Name,
            Email = oauthInfo.Email,
            Image = oauthInfo.Picture,
            Provider = provider,
            ProviderUserId = oauthInfo.Id,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _logger.LogInformation("Created user from OAuth: {UserName} ({Email}) via {Provider}", user.Name, user.Email, provider);
        return user;
    }

    /// <summary>
    /// Generates a unique user ID based on provider and provider user ID.
    /// This ensures the same user from the same provider always gets the same ID.
    /// </summary>
    /// <param name="provider">OAuth provider</param>
    /// <param name="providerUserId">Provider-specific user ID</param>
    /// <returns>Deterministic unique user ID</returns>
    public string GenerateUniqueUserId(string provider, string providerUserId)
    {
        // Create a deterministic ID based on provider and provider user ID
        // This ensures the same user from the same provider always gets the same ID
        var combinedString = $"{provider}:{providerUserId}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedString));
        return Convert.ToHexString(hashBytes)[..32]; // Take first 32 characters for a reasonable ID length
    }

    /// <summary>
    /// Validates if a user ID matches the expected format and provider pattern.
    /// </summary>
    /// <param name="userId">User ID to validate</param>
    /// <param name="provider">Expected provider</param>
    /// <param name="providerUserId">Expected provider user ID</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool ValidateUserId(string userId, string provider, string providerUserId)
    {
        var expectedId = GenerateUniqueUserId(provider, providerUserId);
        return userId.Equals(expectedId, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<User> GetUserByIdOrThrowAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {id} not found.");
        }
        return user;
    }

    // HttpContext-aware methods for controller use
    public async Task<IActionResult> GetByIdAsync(HttpContext httpContext, string id)
    {
        try
        {
            var user = await GetByIdAsync(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { message = "User not found" });
            }

            return new OkObjectResult(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get user by ID error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> GetByEmailAsync(HttpContext httpContext, string email)
    {
        try
        {
            var user = await GetByEmailAsync(email);
            if (user == null)
            {
                return new NotFoundObjectResult(new { message = "User not found" });
            }

            return new OkObjectResult(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get user by email error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> GetAllAsync(HttpContext httpContext, int page, int pageSize)
    {
        try
        {
            var (users, totalCount) = await GetAllWithCountAsync(page, pageSize);
            var response = new 
            { 
                users = users,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get all users error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> GetUsersByIdsAsync(HttpContext httpContext, IEnumerable<string> userIds)
    {
        try
        {
            var users = await GetUsersByIdsAsync(userIds);
            return new OkObjectResult(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get users by IDs error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> SearchUsersAsync(HttpContext httpContext, string searchTerm, int page, int pageSize)
    {
        try
        {
            var (users, totalCount) = await SearchUsersAsync(searchTerm, page, pageSize);
            var response = new 
            { 
                users = users,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                searchTerm = searchTerm
            };
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search users error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> CreateAsync(HttpContext httpContext, CreateUserDto createDto, ModelStateDictionary modelState)
    {
        try
        {
            if (!modelState.IsValid)
            {
                return new BadRequestObjectResult(modelState);
            }

            var user = await CreateAsync(createDto);
            return new CreatedAtActionResult("GetById", null, new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create user error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> UpdateAsync(HttpContext httpContext, string id, UpdateUserDto updateDto, ModelStateDictionary modelState)
    {
        try
        {
            if (!modelState.IsValid)
            {
                return new BadRequestObjectResult(modelState);
            }

            var user = await UpdateAsync(id, updateDto);
            return new OkObjectResult(user);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update user error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> DeleteAsync(HttpContext httpContext, string id)
    {
        try
        {
            await DeleteAsync(id);
            return new NoContentResult();
        }
        catch (ArgumentException ex)
        {
            return new NotFoundObjectResult(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete user error: {Message}", ex.Message);
            return new ObjectResult(new { message = "Internal server error" }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> BanUserAsync(HttpContext httpContext, BanUserRequest request)
    {
        return await _banService.BanUserAsync(httpContext, request);
    }

    public async Task<IActionResult> UnbanUserAsync(HttpContext httpContext, UnbanUserRequest request)
    {
        return await _banService.UnbanUserAsync(httpContext, request);
    }
}
