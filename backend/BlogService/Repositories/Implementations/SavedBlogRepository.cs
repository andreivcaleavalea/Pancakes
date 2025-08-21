using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class SavedBlogRepository : ISavedBlogRepository
{
    private readonly BlogDbContext _context;

    public SavedBlogRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SavedBlog>> GetSavedBlogsByUserIdAsync(string userId)
    {
        return await _context.SavedBlogs
            .Include(sb => sb.BlogPost)
            .Where(sb => sb.UserId == userId)
            .OrderByDescending(sb => sb.SavedAt)
            .ToListAsync();
    }

    public async Task<SavedBlog?> GetSavedBlogAsync(string userId, Guid blogPostId)
    {
        return await _context.SavedBlogs
            .Include(sb => sb.BlogPost)
            .FirstOrDefaultAsync(sb => sb.UserId == userId && sb.BlogPostId == blogPostId);
    }

    public async Task<SavedBlog> SaveBlogAsync(SavedBlog savedBlog)
    {
        _context.SavedBlogs.Add(savedBlog);
        await _context.SaveChangesAsync();
        
        // Reload with blog post included
        return await _context.SavedBlogs
            .Include(sb => sb.BlogPost)
            .FirstOrDefaultAsync(sb => sb.UserId == savedBlog.UserId && sb.BlogPostId == savedBlog.BlogPostId) ?? savedBlog;
    }

    public async Task DeleteSavedBlogAsync(string userId, Guid blogPostId)
    {
        var savedBlog = await _context.SavedBlogs
            .FirstOrDefaultAsync(sb => sb.UserId == userId && sb.BlogPostId == blogPostId);

        if (savedBlog != null)
        {
            _context.SavedBlogs.Remove(savedBlog);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsBookmarkedAsync(string userId, Guid blogPostId)
    {
        return await _context.SavedBlogs
            .AnyAsync(sb => sb.UserId == userId && sb.BlogPostId == blogPostId);
    }

    public async Task<IEnumerable<SavedBlog>> GetUserSavedPostsAsync(string userId)
    {
        return await _context.SavedBlogs
            .Include(sb => sb.BlogPost)
            .Where(sb => sb.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SavedBlog>> GetPostSavesByUsersAsync(Guid postId, IEnumerable<string> userIds)
    {
        return await _context.SavedBlogs
            .Include(sb => sb.BlogPost)
            .Where(sb => sb.BlogPostId == postId && userIds.Contains(sb.UserId))
            .ToListAsync();
    }
}