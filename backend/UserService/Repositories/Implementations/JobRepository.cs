using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;

namespace UserService.Repositories.Implementations;

public class JobRepository : IJobRepository
{
    private readonly UserDbContext _context;

    public JobRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(string id)
    {
        return await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<Job>> GetByUserIdAsync(string userId)
    {
        return await _context.Jobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.StartDate)
            .ToListAsync();
    }

    public async Task<Job> CreateAsync(Job job)
    {
        job.Id = Guid.NewGuid().ToString();
        job.CreatedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<Job> UpdateAsync(Job job)
    {
        job.UpdatedAt = DateTime.UtcNow;
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task DeleteAsync(string id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job != null)
        {
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Jobs.AnyAsync(j => j.Id == id);
    }
} 