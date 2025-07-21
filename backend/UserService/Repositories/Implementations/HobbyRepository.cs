using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;

namespace UserService.Repositories.Implementations;

public class HobbyRepository : IHobbyRepository
{
    private readonly UserDbContext _context;

    public HobbyRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<Hobby?> GetByIdAsync(string id)
    {
        return await _context.Hobbies.FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IEnumerable<Hobby>> GetByUserIdAsync(string userId)
    {
        return await _context.Hobbies
            .Where(h => h.UserId == userId)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Hobby> CreateAsync(Hobby hobby)
    {
        hobby.Id = Guid.NewGuid().ToString();
        hobby.CreatedAt = DateTime.UtcNow;
        hobby.UpdatedAt = DateTime.UtcNow;
        
        _context.Hobbies.Add(hobby);
        await _context.SaveChangesAsync();
        return hobby;
    }

    public async Task<Hobby> UpdateAsync(Hobby hobby)
    {
        hobby.UpdatedAt = DateTime.UtcNow;
        _context.Hobbies.Update(hobby);
        await _context.SaveChangesAsync();
        return hobby;
    }

    public async Task DeleteAsync(string id)
    {
        var hobby = await _context.Hobbies.FindAsync(id);
        if (hobby != null)
        {
            _context.Hobbies.Remove(hobby);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Hobbies.AnyAsync(h => h.Id == id);
    }
} 