using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;

namespace UserService.Repositories.Implementations
{
    public class BanRepository : IBanRepository
    {
        private readonly UserDbContext _context;

        public BanRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Ban?> GetByIdAsync(string id)
        {
            return await _context.Bans
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Ban?> GetActiveBanAsync(string userId)
        {
            return await _context.Bans
                .Include(b => b.User)
                .Where(b => b.UserId == userId && 
                           b.IsActive && 
                           (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Ban>> GetBanHistoryAsync(string userId)
        {
            return await _context.Bans
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ban>> GetAllActiveBansAsync()
        {
            return await _context.Bans
                .Include(b => b.User)
                .Where(b => b.IsActive && 
                           (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ban> CreateAsync(Ban ban)
        {
            ban.Id = Guid.NewGuid().ToString();
            ban.CreatedAt = DateTime.UtcNow;
            ban.UpdatedAt = DateTime.UtcNow;
            
            _context.Bans.Add(ban);
            await _context.SaveChangesAsync();
            return ban;
        }

        public async Task<Ban> UpdateAsync(Ban ban)
        {
            ban.UpdatedAt = DateTime.UtcNow;
            _context.Bans.Update(ban);
            await _context.SaveChangesAsync();
            return ban;
        }

        public async Task DeleteAsync(string id)
        {
            var ban = await GetByIdAsync(id);
            if (ban != null)
            {
                _context.Bans.Remove(ban);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasActiveBanAsync(string userId)
        {
            return await _context.Bans
                .AnyAsync(b => b.UserId == userId && 
                              b.IsActive && 
                              (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow));
        }

        public async Task<int> GetActiveBansCountAsync()
        {
            return await _context.Bans
                .CountAsync(b => b.IsActive && 
                               (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow));
        }

        public async Task<IEnumerable<Ban>> GetExpiringBansAsync(DateTime cutoffTime)
        {
            return await _context.Bans
                .Where(b => b.IsActive && 
                           b.ExpiresAt.HasValue && 
                           b.ExpiresAt <= cutoffTime)
                .ToListAsync();
        }
    }
}