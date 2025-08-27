using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Models.Requests;
using UserService.Repositories.Interfaces;

namespace UserService.Repositories.Implementations;

public class PersonalPageSettingsRepository : IPersonalPageSettingsRepository
{
    private readonly UserDbContext _context;

    public PersonalPageSettingsRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<PersonalPageSettings?> GetByUserIdAsync(string userId)
    {
        return await _context.PersonalPageSettings
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<PersonalPageSettings?> GetByPageSlugAsync(string pageSlug)
    {
        return await _context.PersonalPageSettings
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PageSlug == pageSlug && p.IsPublic);
    }

    public async Task<PersonalPageSettings> CreateAsync(PersonalPageSettings settings)
    {
        settings.Id = Guid.NewGuid().ToString();
        settings.CreatedAt = DateTime.UtcNow;
        settings.UpdatedAt = DateTime.UtcNow;
        
        _context.PersonalPageSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<PersonalPageSettings> UpdateAsync(PersonalPageSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _context.PersonalPageSettings.Update(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task DeleteAsync(string id)
    {
        var settings = await _context.PersonalPageSettings.FindAsync(id);
        if (settings != null)
        {
            _context.PersonalPageSettings.Remove(settings);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        return await _context.PersonalPageSettings.AnyAsync(p => p.UserId == userId);
    }

    public async Task<bool> PageSlugExistsAsync(string pageSlug, string? excludeUserId = null)
    {
        var query = _context.PersonalPageSettings.Where(p => p.PageSlug == pageSlug);
        
        if (!string.IsNullOrEmpty(excludeUserId))
        {
            query = query.Where(p => p.UserId != excludeUserId);
        }
        
        return await query.AnyAsync();
    }

    public async Task<(List<PersonalPageSettings> settings, int totalCount)> GetPublicPaginatedAsync(PortfolioQueryParameters parameters)
    {
        var query = _context.PersonalPageSettings
            .Include(p => p.User)
            .Where(p => p.IsPublic && !string.IsNullOrEmpty(p.PageSlug));

        // Apply search filter
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(p => p.User.Name.Contains(parameters.Search) || 
                                   (p.User.Bio != null && p.User.Bio.Contains(parameters.Search)));
        }

        // Apply sorting
        query = parameters.SortBy.ToLower() switch
        {
            "name" => parameters.SortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.User.Name)
                : query.OrderBy(p => p.User.Name),
            "createdat" => parameters.SortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var settings = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return (settings, totalCount);
    }
} 