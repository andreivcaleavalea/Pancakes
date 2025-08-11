using System;
using System.Collections.Generic;
using BlogService.Data;
using BlogService.Models.Entities;

namespace BlogService.Tests.TestUtilities;

public static class BlogPostTestData
{
    public static List<BlogPost> CreateSeedPosts(DateTime now)
    {
        return new List<BlogPost>
        {
            new()
            {
                Title = "Apple Pie",
                Content = "Recipe",
                AuthorId = "u1",
                Status = PostStatus.Published,
                CreatedAt = now.AddDays(-7),
                UpdatedAt = now.AddDays(-7).AddHours(1),
                PublishedAt = now.AddDays(-6)
            },
            new()
            {
                Title = "Banana Bread",
                Content = "Delicious banana",
                AuthorId = "u1",
                Status = PostStatus.Draft,
                CreatedAt = now.AddDays(-6),
                UpdatedAt = now.AddDays(-6).AddHours(1),
                PublishedAt = null
            },
            new()
            {
                Title = "Carrot Soup",
                Content = "Healthy soup",
                AuthorId = "u2",
                Status = PostStatus.Published,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5).AddHours(1),
                PublishedAt = now.AddDays(-4)
            },
            new()
            {
                Title = "Donut",
                Content = "Sweet donut",
                AuthorId = "u2",
                Status = PostStatus.Published,
                CreatedAt = now.AddDays(-4),
                UpdatedAt = now.AddDays(-4).AddHours(1),
                PublishedAt = null
            },
            new()
            {
                Title = "Eclair",
                Content = "Chocolate eclair",
                AuthorId = "u3",
                Status = PostStatus.Published,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-3).AddHours(1),
                PublishedAt = now.AddDays(-2)
            },
            new()
            {
                Title = "Falafel",
                Content = "Vegan falafel",
                AuthorId = "u3",
                Status = PostStatus.Draft,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2).AddHours(1),
                PublishedAt = null
            },
            new()
            {
                Title = "Granola",
                Content = "Healthy breakfast",
                AuthorId = "u1",
                Status = PostStatus.Published,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1).AddHours(1),
                PublishedAt = null
            },
            new()
            {
                Title = "Honey Cake",
                Content = "Sweet honey cake",
                AuthorId = "u2",
                Status = PostStatus.Published,
                CreatedAt = now.AddDays(-8),
                UpdatedAt = now.AddDays(-8).AddHours(1),
                PublishedAt = now.AddDays(-7)
            }
        };
    }

    public static BlogDbContext CreateContextWithSeed(out List<BlogPost> seeded, DateTime? now = null)
    {
        var actualNow = now ?? DateTime.UtcNow;
        var posts = CreateSeedPosts(actualNow);
        seeded = posts;

        var context = TestDbContextFactory.Create(ctx => ctx.BlogPosts.AddRange(posts));
        return context;
    }
}


