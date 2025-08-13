using BlogService.Models.Requests;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using Microsoft.Extensions.Caching.Memory;

namespace BlogService.Tests.Services;

public class TagServiceTests
{
    private static TagService CreateService(out Mock<IBlogPostRepository> repo, out IMemoryCache cache)
    {
        repo = new Mock<IBlogPostRepository>(MockBehavior.Strict);
        cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TagService>>();
        return new TagService(repo.Object, logger.Object, cache);
    }

    [Fact]
    public async Task GetPopularTags_Cache_Miss_Then_Hit()
    {
        var service = CreateService(out var repo, out var cache);

        var posts = new List<BlogService.Models.Entities.BlogPost>
        {
            new() { Tags = new List<string>{"React","CSharp","react"} },
            new() { Tags = new List<string>{"csharp","dotnet"} }
        };
        repo.Setup(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()))
            .ReturnsAsync((posts, posts.Count));

        var first = (await service.GetPopularTagsAsync(2)).ToList();
        first.Select(t => t.ToLower()).Should().Equal("react", "csharp");

        // Second call should be served from cache; repository not called again
        var second = (await service.GetPopularTagsAsync(2)).ToList();
        second.Should().Equal(first);
        repo.Verify(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()), Times.Once);
    }

    [Fact]
    public async Task SearchTags_Cache_Miss_Then_Hit_And_Empty_On_Short_Query()
    {
        var service = CreateService(out var repo, out var cache);

        var posts = new List<BlogService.Models.Entities.BlogPost>
        {
            new() { Tags = new List<string>{"react","redux","ruby"} },
            new() { Tags = new List<string>{"rust","rails"} }
        };
        repo.Setup(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()))
            .ReturnsAsync((posts, posts.Count));

        var miss = (await service.SearchTagsAsync("ru", 3)).ToList();
        miss.Should().HaveCount(2);
        miss.Select(s => s.ToLower()).Should().BeEquivalentTo(new[] { "ruby", "rust" });

        // Second call served from cache
        var hit = (await service.SearchTagsAsync("ru", 3)).ToList();
        hit.Should().Equal(miss);
        repo.Verify(r => r.GetAllAsync(It.IsAny<BlogPostQueryParameters>()), Times.Once);

        // Short query returns empty
        var empty = await service.SearchTagsAsync("r", 3);
        empty.Should().BeEmpty();
    }
}


