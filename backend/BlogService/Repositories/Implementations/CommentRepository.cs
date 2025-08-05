using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class CommentRepository : ICommentRepository
{
    private readonly BlogDbContext _context;

    public CommentRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.BlogPost)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comment>> GetByBlogPostIdAsync(Guid blogPostId)
    {
        return await _context.Comments
            .Where(c => c.BlogPostId == blogPostId && c.ParentCommentId == null) // Only top-level comments
            .Include(c => c.Replies.OrderBy(r => r.CreatedAt))
                .ThenInclude(r => r.Replies.OrderBy(rr => rr.CreatedAt))
                .ThenInclude(rr => rr.Replies.OrderBy(rrr => rrr.CreatedAt)) // Support up to 4 levels deep
            .OrderByDescending(c => c.CreatedAt) // Top-level comments ordered newest first
            .ToListAsync();
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        // Reload the comment with its relationships for proper DTO mapping
        return await _context.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == comment.Id) ?? comment;
    }

    public async Task<Comment> UpdateAsync(Comment comment)
    {
        comment.UpdatedAt = DateTime.UtcNow;
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Comments.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> HasRepliesAsync(Guid commentId)
    {
        return await _context.Comments.AnyAsync(c => c.ParentCommentId == commentId);
    }
} 