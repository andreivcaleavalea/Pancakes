using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;

namespace UserService.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByProviderAndProviderUserIdAsync(string provider, string providerUserId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => 
            u.Provider == provider && u.ProviderUserId == providerUserId);
    }

    public async Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var skip = (page - 1) * pageSize;
        return await _context.Users
            .OrderBy(u => u.Name)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByProviderAndProviderUserIdAsync(string provider, string providerUserId)
    {
        return await _context.Users.AnyAsync(u => 
            u.Provider == provider && u.ProviderUserId == providerUserId);
    }
}
