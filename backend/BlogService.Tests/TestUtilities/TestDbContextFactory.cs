using System;
using Microsoft.EntityFrameworkCore;
using BlogService.Data;

namespace BlogService.Tests.TestUtilities;

public static class TestDbContextFactory
{
    public static BlogDbContext Create(Action<BlogDbContext>? seed = null)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase($"BlogDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        var context = new BlogDbContext(options);

        if (seed != null)
        {
            seed(context);
            context.SaveChanges();
        }

        return context;
    }
}

