using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;

namespace UserService.Repositories.Implementations;

public class EducationRepository : IEducationRepository
{
    private readonly UserDbContext _context;

    public EducationRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<Education?> GetByIdAsync(string id)
    {
        return await _context.Educations.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Education>> GetByUserIdAsync(string userId)
    {
        return await _context.Educations
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<Education> CreateAsync(Education education)
    {
        education.Id = Guid.NewGuid().ToString();
        education.CreatedAt = DateTime.UtcNow;
        education.UpdatedAt = DateTime.UtcNow;
        
        _context.Educations.Add(education);
        await _context.SaveChangesAsync();
        return education;
    }

    public async Task<Education> UpdateAsync(Education education)
    {
        education.UpdatedAt = DateTime.UtcNow;
        _context.Educations.Update(education);
        await _context.SaveChangesAsync();
        return education;
    }

    public async Task DeleteAsync(string id)
    {
        var education = await _context.Educations.FindAsync(id);
        if (education != null)
        {
            _context.Educations.Remove(education);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Educations.AnyAsync(e => e.Id == id);
    }
} 