using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class PostRatingRepository : IPostRatingRepository
{
    private readonly BlogDbContext _context;

    public PostRatingRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<PostRating?> GetByIdAsync(Guid id)
    {
        return await _context.PostRatings
            .Include(pr => pr.BlogPost)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<PostRating?> GetByBlogPostAndUserAsync(Guid blogPostId, string userId)
    {
        return await _context.PostRatings
            .FirstOrDefaultAsync(pr => pr.BlogPostId == blogPostId && pr.UserId == userId);
    }

    public async Task<IEnumerable<PostRating>> GetByBlogPostIdAsync(Guid blogPostId)
    {
        return await _context.PostRatings
            .Where(pr => pr.BlogPostId == blogPostId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();
    }

    public async Task<PostRating> CreateAsync(PostRating rating)
    {
        _context.PostRatings.Add(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<PostRating> UpdateAsync(PostRating rating)
    {
        rating.UpdatedAt = DateTime.UtcNow;
        _context.PostRatings.Update(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task DeleteAsync(Guid id)
    {
        var rating = await _context.PostRatings.FindAsync(id);
        if (rating != null)
        {
            _context.PostRatings.Remove(rating);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.PostRatings.AnyAsync(pr => pr.Id == id);
    }

    public async Task<decimal> GetAverageRatingAsync(Guid blogPostId)
    {
        var ratings = await _context.PostRatings
            .Where(pr => pr.BlogPostId == blogPostId)
            .Select(pr => pr.Rating)
            .ToListAsync();

        return ratings.Any() ? ratings.Average() : 0;
    }

    public async Task<int> GetTotalRatingsAsync(Guid blogPostId)
    {
        return await _context.PostRatings
            .CountAsync(pr => pr.BlogPostId == blogPostId);
    }

    public async Task<Dictionary<decimal, int>> GetRatingDistributionAsync(Guid blogPostId)
    {
        var ratings = await _context.PostRatings
            .Where(pr => pr.BlogPostId == blogPostId)
            .GroupBy(pr => pr.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync();

        return ratings.ToDictionary(r => r.Rating, r => r.Count);
    }

    public async Task<IEnumerable<PostRating>> GetUserRatingsAsync(string userId)
    {
        return await _context.PostRatings
            .Include(pr => pr.BlogPost)
            .Where(pr => pr.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PostRating>> GetPostRatingsByUsersAsync(Guid postId, IEnumerable<string> userIds)
    {
        return await _context.PostRatings
            .Include(pr => pr.BlogPost)
            .Where(pr => pr.BlogPostId == postId && userIds.Contains(pr.UserId))
            .ToListAsync();
    }
} 