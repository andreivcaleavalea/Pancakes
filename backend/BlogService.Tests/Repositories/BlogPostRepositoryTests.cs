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
}


