using AutoMapper;
using UserService.Models;
using UserService.Models.DTOs;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly UserManagementService _userManagementService;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        UserManagementService userManagementService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _userManagementService = userManagementService;
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
        user.Id = _userManagementService.GenerateUniqueUserId(createDto.Provider, createDto.ProviderUserId);
        
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
        // Check if user exists by provider and provider user ID
        var existingUser = await _userRepository.GetByProviderAndProviderUserIdAsync(provider, oauthInfo.Id);
        
        if (existingUser != null)
        {
            // Update existing user with latest OAuth info and last login
            existingUser.Name = oauthInfo.Name;
            existingUser.Email = oauthInfo.Email;
            existingUser.Image = oauthInfo.Picture;
            existingUser.LastLoginAt = DateTime.UtcNow;
            
            var updatedUser = await _userRepository.UpdateAsync(existingUser);
            return _mapper.Map<UserDto>(updatedUser);
        }
        else
        {
            // Create new user from OAuth info
            var userId = _userManagementService.GenerateUniqueUserId(provider, oauthInfo.Id);
            
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

    private async Task<User> GetUserByIdOrThrowAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {id} not found.");
        }
        return user;
    }
}
