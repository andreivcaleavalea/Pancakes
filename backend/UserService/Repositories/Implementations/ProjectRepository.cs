using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;

namespace UserService.Repositories.Implementations;

public class ProjectRepository : IProjectRepository
{
    private readonly UserDbContext _context;

    public ProjectRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(string id)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetByUserIdAsync(string userId)
    {
        return await _context.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<Project> CreateAsync(Project project)
    {
        project.Id = Guid.NewGuid().ToString();
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task DeleteAsync(string id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Projects.AnyAsync(p => p.Id == id);
    }
} 