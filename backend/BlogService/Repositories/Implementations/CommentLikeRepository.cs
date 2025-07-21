using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class CommentLikeRepository : ICommentLikeRepository
{
    private readonly BlogDbContext _context;

    public CommentLikeRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<CommentLike?> GetByIdAsync(Guid id)
    {
        return await _context.CommentLikes
            .Include(cl => cl.Comment)
            .FirstOrDefaultAsync(cl => cl.Id == id);
    }

    public async Task<CommentLike?> GetByCommentAndUserAsync(Guid commentId, string userIdentifier)
    {
        return await _context.CommentLikes
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserIdentifier == userIdentifier);
    }

    public async Task<IEnumerable<CommentLike>> GetByCommentIdAsync(Guid commentId)
    {
        return await _context.CommentLikes
            .Where(cl => cl.CommentId == commentId)
            .OrderByDescending(cl => cl.CreatedAt)
            .ToListAsync();
    }

    public async Task<CommentLike> CreateAsync(CommentLike like)
    {
        _context.CommentLikes.Add(like);
        await _context.SaveChangesAsync();
        return like;
    }

    public async Task<CommentLike> UpdateAsync(CommentLike like)
    {
        like.UpdatedAt = DateTime.UtcNow;
        _context.CommentLikes.Update(like);
        await _context.SaveChangesAsync();
        return like;
    }

    public async Task DeleteAsync(Guid id)
    {
        var like = await _context.CommentLikes.FindAsync(id);
        if (like != null)
        {
            _context.CommentLikes.Remove(like);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.CommentLikes.AnyAsync(cl => cl.Id == id);
    }

    public async Task<int> GetLikeCountAsync(Guid commentId)
    {
        return await _context.CommentLikes
            .CountAsync(cl => cl.CommentId == commentId && cl.IsLike);
    }

    public async Task<int> GetDislikeCountAsync(Guid commentId)
    {
        return await _context.CommentLikes
            .CountAsync(cl => cl.CommentId == commentId && !cl.IsLike);
    }
} 