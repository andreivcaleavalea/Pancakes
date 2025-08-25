using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Repositories.Implementations;

public class BlogPostRepository : IBlogPostRepository
{
    private readonly BlogDbContext _context;

    public BlogPostRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<BlogPost?> GetByIdAsync(Guid id)
    {
        return await _context.BlogPosts.FirstOrDefaultAsync(bp => bp.Id == id);
    }

    public async Task<(IEnumerable<BlogPost> posts, int totalCount)> GetAllAsync(BlogPostQueryParameters parameters)
    {
        var query = _context.BlogPosts.AsQueryable();
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(bp => bp.Title.ToLower().Contains(parameters.Search.ToLower()) || bp.Content.ToLower().Contains(parameters.Search.ToLower()));
        }
        if (!string.IsNullOrEmpty(parameters.AuthorId))
        {
            query = query.Where(bp => bp.AuthorId == parameters.AuthorId);
        }
        if (!string.IsNullOrEmpty(parameters.ExcludeAuthorId))
        {
            query = query.Where(bp => bp.AuthorId != parameters.ExcludeAuthorId);
        }
        if (parameters.Status.HasValue)
        {
            query = query.Where(bp => bp.Status == parameters.Status);
        }
        if (parameters.DateFrom.HasValue)
        {
            query = query.Where(bp => bp.CreatedAt >= parameters.DateFrom);
        }
        if (parameters.DateTo.HasValue)
        {
            query = query.Where(bp => bp.CreatedAt <= parameters.DateTo);
        }
        
        // Sorting
        if (!string.IsNullOrEmpty(parameters.SortBy))
        {
            query = parameters.SortBy.ToLower() switch
            {
                "publishedat" => parameters.SortOrder?.ToLower() == "desc" 
                    ? query.Where(bp => bp.PublishedAt.HasValue).OrderByDescending(bp => bp.PublishedAt)
                    : query.Where(bp => bp.PublishedAt.HasValue).OrderBy(bp => bp.PublishedAt),
                "createdat" => parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(bp => bp.CreatedAt)
                    : query.OrderBy(bp => bp.CreatedAt),
                "updatedat" => parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(bp => bp.UpdatedAt)
                    : query.OrderBy(bp => bp.UpdatedAt),
                "title" => parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(bp => bp.Title)
                    : query.OrderBy(bp => bp.Title),
                _ => query.OrderByDescending(bp => bp.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(bp => bp.CreatedAt);
        }

        // Tag filtering - temporarily using client-side evaluation
        if (parameters.Tags != null && parameters.Tags.Any())
        {
            Console.WriteLine($"🔍 Tag filtering - Requested tags: [{string.Join(", ", parameters.Tags)}]");
            
            // Convert to list for client-side evaluation
            var allPosts = await query.ToListAsync();
            Console.WriteLine($"📊 Total posts before tag filtering: {allPosts.Count}");
            
            // Filter posts that contain ALL specified tags (AND logic)
            var filteredPosts = allPosts.Where(bp => 
            {
                if (bp.Tags == null || !bp.Tags.Any()) 
                {
                    return false;
                }
                
                // Check if this post contains ALL requested tags (case-insensitive)
                var postContainsAllTags = parameters.Tags.All(requestedTag => 
                    bp.Tags.Any(postTag => 
                        string.Equals(postTag, requestedTag, StringComparison.OrdinalIgnoreCase)
                    )
                );
                
                if (postContainsAllTags)
                {
                    Console.WriteLine($"✅ Post '{bp.Title}' matches - Has tags: [{string.Join(", ", bp.Tags)}]");
                }
                
                return postContainsAllTags;
            }).ToList();
            
            Console.WriteLine($"🎯 Posts after tag filtering: {filteredPosts.Count}");
            
            var totalCount = filteredPosts.Count;
            var posts = filteredPosts.Skip((parameters.Page - 1) * parameters.PageSize).Take(parameters.PageSize);
            return (posts, totalCount);
        }
        
        // Normal execution (no tag filtering)
        var normalTotalCount = await query.CountAsync();
        var normalPosts = await query.Skip((parameters.Page - 1) * parameters.PageSize).Take(parameters.PageSize).ToListAsync();
        return (normalPosts, normalTotalCount);
    }

    // Today's special - returns the newest post
    public async Task<IEnumerable<BlogPost>> GetFeaturedAsync(int count = 1)
    {
        return await _context.BlogPosts.Where(bp => bp.Status == PostStatus.Published)
            .OrderByDescending(bp => bp.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    // Popular recipes - returns the 3 newest posts
    public async Task<IEnumerable<BlogPost>> GetPopularAsync(int count = 3)
    {
        return await _context.BlogPosts.Where(bp => bp.Status == PostStatus.Published)
            .OrderByDescending(bp => bp.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<BlogPost> CreateAsync(BlogPost blogPost)
    {
        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();
        return blogPost;
    }

    public async Task<BlogPost> UpdateAsync(BlogPost blogPost)
    {
        _context.BlogPosts.Update(blogPost);
        await _context.SaveChangesAsync();
        return blogPost;
    }

    public async Task DeleteAsync(Guid id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost != null)
        {
            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.BlogPosts.AnyAsync(bp => bp.Id == id);
    }

    public async Task<(IEnumerable<BlogPost> posts, int totalCount)> GetFriendsPostsAsync(IEnumerable<string> friendUserIds, int page = 1, int pageSize = 10)
    {
        var query = _context.BlogPosts
            .Where(bp => bp.Status == PostStatus.Published && friendUserIds.Contains(bp.AuthorId))
            .OrderByDescending(bp => bp.PublishedAt ?? bp.CreatedAt);

        var totalCount = await query.CountAsync();
        
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost != null)
        {
            blogPost.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalPublishedCountAsync()
    {
        return await _context.BlogPosts
            .CountAsync(bp => bp.Status == PostStatus.Published);
    }
}
