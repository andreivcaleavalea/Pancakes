using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Models.Requests;
using BlogService.Repositories.Implementations;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Repositories;

public class BlogPostRepositoryTests
{

    [Fact]
    public async Task GetAll_Search_Matches_Title()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { Search = "honey", PageSize = 50 };
        var (posts, totalCount) = await repo.GetAllAsync(parameters);

        posts.Should().NotBeNull();
        posts.Should().OnlyContain(p =>
            p.Title.Contains("honey", StringComparison.OrdinalIgnoreCase) ||
            p.Content.Contains("honey", StringComparison.OrdinalIgnoreCase));
        totalCount.Should().Be(posts.Count());
    }

    [Fact]
    public async Task GetAll_Search_Matches_Content()
    {
        using var context = BlogPostTestData.CreateContextWithSeed(out _);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { Search = "delicious", PageSize = 50 };
        var (posts, totalCount) = await repo.GetAllAsync(parameters);

        posts.Should().NotBeEmpty();
        posts.Should().OnlyContain(p => p.Title.Contains("delicious", StringComparison.OrdinalIgnoreCase) || p.Content.Contains("delicious", StringComparison.OrdinalIgnoreCase));
        totalCount.Should().Be(posts.Count());
    }

    [Fact]
    public async Task GetAll_Filter_By_AuthorId()
    {
        using var context = BlogPostTestData.CreateContextWithSeed(out _);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { AuthorId = "u2", PageSize = 50 };
        var (posts, totalCount) = await repo.GetAllAsync(parameters);

        posts.Should().OnlyContain(p => p.AuthorId == "u2");
        totalCount.Should().Be(posts.Count());
    }

    [Fact]
    public async Task GetAll_Filter_By_Status()
    {
        using var context = BlogPostTestData.CreateContextWithSeed(out _);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { Status = PostStatus.Published, PageSize = 50 };
        var (posts, totalCount) = await repo.GetAllAsync(parameters);

        posts.Should().OnlyContain(p => p.Status == PostStatus.Published);
        totalCount.Should().Be(posts.Count());
    }

    [Fact]
    public async Task GetAll_Filter_By_DateRange()
    {
        using var context = BlogPostTestData.CreateContextWithSeed(out _);
        var repo = new BlogPostRepository(context);

        var dateFrom = DateTime.UtcNow.AddDays(-5.5);
        var dateTo = DateTime.UtcNow.AddDays(-3.5);

        var parameters = new BlogPostQueryParameters { DateFrom = dateFrom, DateTo = dateTo, PageSize = 50 };
        var (posts, totalCount) = await repo.GetAllAsync(parameters);

        posts.Should().OnlyContain(p => p.CreatedAt >= dateFrom && p.CreatedAt <= dateTo);
        totalCount.Should().Be(posts.Count());
    }

    [Theory]
    [InlineData("publishedAt", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("updatedAt", "desc")]
    [InlineData("title", "asc")]
    public async Task GetAll_Sorts_As_Expected(string sortBy, string sortOrder)
    {
        List<BlogPost> seeded;
        var now = DateTime.UtcNow;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded, now);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { SortBy = sortBy, SortOrder = sortOrder, PageSize = 100 };
        var (posts, _) = await repo.GetAllAsync(parameters);

        var expected = seeded!;
        switch (sortBy.ToLower())
        {
            case "publishedat":
                expected = sortOrder.ToLower() == "desc"
                    ? seeded!.Where(p => p.PublishedAt.HasValue).OrderByDescending(p => p.PublishedAt).ToList()
                    : seeded!.Where(p => p.PublishedAt.HasValue).OrderBy(p => p.PublishedAt).ToList();
                break;
            case "createdat":
                expected = sortOrder.ToLower() == "desc"
                    ? seeded!.OrderByDescending(p => p.CreatedAt).ToList()
                    : seeded!.OrderBy(p => p.CreatedAt).ToList();
                break;
            case "updatedat":
                expected = sortOrder.ToLower() == "desc"
                    ? seeded!.OrderByDescending(p => p.UpdatedAt).ToList()
                    : seeded!.OrderBy(p => p.UpdatedAt).ToList();
                break;
            case "title":
                expected = sortOrder.ToLower() == "desc"
                    ? seeded!.OrderByDescending(p => p.Title, StringComparer.Ordinal).ToList()
                    : seeded!.OrderBy(p => p.Title, StringComparer.Ordinal).ToList();
                break;
        }

        posts.Select(p => p.Title).Should().Equal(expected.Select(e => e.Title));
    }

    [Fact]
    public async Task GetAll_Default_Sort_Is_CreatedAt_Desc_When_SortBy_Empty()
    {
        List<BlogPost> seeded;
        var now = DateTime.UtcNow;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded, now);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { SortBy = string.Empty, PageSize = 100 };
        var (posts, _) = await repo.GetAllAsync(parameters);

        var expected = seeded!.OrderByDescending(p => p.CreatedAt).Select(p => p.Title).ToList();
        posts.Select(p => p.Title).Should().Equal(expected);
    }

    [Fact]
    public async Task Pagination_Slices_And_Returns_TotalCount()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { Page = 2, PageSize = 2 };
        var (posts, totalCount) = await repo.GetAllAsync(parameters);

        totalCount.Should().Be(seeded!.Count);

        var expectedTitles = seeded!
            .OrderByDescending(p => p.CreatedAt)
            .Skip(2)
            .Take(2)
            .Select(p => p.Title)
            .ToList();

        posts.Select(p => p.Title).Should().Equal(expectedTitles);
    }

    [Fact]
    public async Task Featured_Returns_Published_Only_And_Honors_Count()
    {
        using var context = BlogPostTestData.CreateContextWithSeed(out _);
        var repo = new BlogPostRepository(context);

        var results = await repo.GetFeaturedAsync(count: 2);
        results.Should().OnlyContain(p => p.Status == PostStatus.Published);

        var expected = context.BlogPosts.AsQueryable()
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.CreatedAt)
            .Take(2)
            .Select(p => p.Title)
            .ToList();

        results.Select(p => p.Title).Should().Equal(expected);
    }

    [Fact]
    public async Task Popular_Returns_Published_Only_And_Honors_Count()
    {
        using var context = BlogPostTestData.CreateContextWithSeed(out _);
        var repo = new BlogPostRepository(context);

        var results = await repo.GetPopularAsync(count: 3);
        results.Should().OnlyContain(p => p.Status == PostStatus.Published);

        var expected = context.BlogPosts.AsQueryable()
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .Select(p => p.Title)
            .ToList();

        results.Select(p => p.Title).Should().Equal(expected);
    }

    [Fact]
    public async Task FriendsPosts_Filters_Orders_Paginates_And_Returns_TotalCount()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var friendIds = new[] { "u1", "u3" };
        var (posts, totalCount) = await repo.GetFriendsPostsAsync(friendIds, page: 1, pageSize: 2);

        var expectedAll = seeded!
            .Where(p => p.Status == PostStatus.Published && friendIds.Contains(p.AuthorId))
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToList();

        totalCount.Should().Be(expectedAll.Count);
        posts.Select(p => p.Title).Should().Equal(expectedAll.Take(2).Select(p => p.Title));
    }

    [Fact]
    public async Task GetById_Found_And_NotFound()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var found = await repo.GetByIdAsync(seeded.First().Id);
        found.Should().NotBeNull();

        var notFound = await repo.GetByIdAsync(Guid.NewGuid());
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_Returns_True_And_False()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var exists = await repo.ExistsAsync(seeded.Last().Id);
        exists.Should().BeTrue();

        var notExists = await repo.ExistsAsync(Guid.NewGuid());
        notExists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Removes_When_Exists()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var id = seeded.First().Id;
        await repo.DeleteAsync(id);

        context.BlogPosts.Any(p => p.Id == id).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NoOp_When_NotFound()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var before = context.BlogPosts.Count();
        await repo.DeleteAsync(Guid.NewGuid());
        var after = context.BlogPosts.Count();
        after.Should().Be(before);
    }

    [Fact]
    public async Task GetAll_Default_Sort_When_SortBy_Null()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { SortBy = null!, PageSize = 100 };
        var (posts, _) = await repo.GetAllAsync(parameters);

        var expected = seeded.OrderByDescending(p => p.CreatedAt).Select(p => p.Title).ToList();
        posts.Select(p => p.Title).Should().Equal(expected);
    }

    [Fact]
    public async Task GetAll_Unknown_SortBy_Falls_Back_To_CreatedAt_Desc()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var parameters = new BlogPostQueryParameters { SortBy = "invalid-field", PageSize = 100 };
        var (posts, _) = await repo.GetAllAsync(parameters);

        var expected = seeded.OrderByDescending(p => p.CreatedAt).Select(p => p.Title).ToList();
        posts.Select(p => p.Title).Should().Equal(expected);
    }

    [Fact]
    public async Task GetAll_DateFrom_Only_And_DateTo_Only()
    {
        var now = DateTime.UtcNow;
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded, now);
        var repo = new BlogPostRepository(context);

        var dateFrom = now.AddDays(-5.5);
        var (fromPosts, _) = await repo.GetAllAsync(new BlogPostQueryParameters { DateFrom = dateFrom, PageSize = 100 });
        fromPosts.Should().OnlyContain(p => p.CreatedAt >= dateFrom);

        var dateTo = now.AddDays(-3.5);
        var (toPosts, _) = await repo.GetAllAsync(new BlogPostQueryParameters { DateTo = dateTo, PageSize = 100 });
        toPosts.Should().OnlyContain(p => p.CreatedAt <= dateTo);
    }

    [Fact]
    public async Task GetAll_Tags_AND_Filter_Paginates_And_Returns_TotalCount()
    {
        using var context = TestDbContextFactory.Create(ctx =>
        {
            ctx.BlogPosts.AddRange(new[]
            {
                new BlogPost { Title = "P1", Content = "c", AuthorId = "u", Status = PostStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-2), Tags = new List<string>{"vegan","quick"} },
                new BlogPost { Title = "P2", Content = "c", AuthorId = "u", Status = PostStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-1), Tags = new List<string>{"vegan"} },
                new BlogPost { Title = "P3", Content = "c", AuthorId = "u", Status = PostStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-3), Tags = new List<string>{"vegan","quick","glutenfree"} },
                new BlogPost { Title = "P4", Content = "c", AuthorId = "u", Status = PostStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-4), Tags = new List<string>{"quick"} }
            });
        });

        var repo = new BlogPostRepository(context);
        var parameters = new BlogPostQueryParameters { Tags = new List<string>{"vegan","quick"}, Page = 1, PageSize = 1 };
        var (page1, total1) = await repo.GetAllAsync(parameters);

        total1.Should().Be(2);
        page1.Should().ContainSingle();
        page1.First().Title.Should().BeOneOf("P1", "P3");

        var (page2, total2) = await repo.GetAllAsync(new BlogPostQueryParameters { Tags = new List<string>{"vegan","quick"}, Page = 2, PageSize = 1 });
        total2.Should().Be(2);
        page2.Should().ContainSingle();
        page2.First().Title.Should().BeOneOf("P1", "P3");
        page2.First().Title.Should().NotBe(page1.First().Title);
    }

    [Fact]
    public async Task GetAll_SortBy_PublishedAt_Asc_Excludes_Null()
    {
        List<BlogPost> seeded;
        using var context = BlogPostTestData.CreateContextWithSeed(out seeded);
        var repo = new BlogPostRepository(context);

        var (posts, _) = await repo.GetAllAsync(new BlogPostQueryParameters { SortBy = "publishedAt", SortOrder = "asc", PageSize = 100 });
        posts.Should().OnlyContain(p => p.PublishedAt.HasValue);

        var expected = seeded.Where(p => p.PublishedAt.HasValue).OrderBy(p => p.PublishedAt).Select(p => p.Title).ToList();
        posts.Select(p => p.Title).Should().Equal(expected);
    }
}


