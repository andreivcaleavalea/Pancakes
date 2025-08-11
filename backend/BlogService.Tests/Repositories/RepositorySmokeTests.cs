using BlogService.Data;
using BlogService.Models.Entities;
using BlogService.Repositories.Implementations;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Repositories;

public class RepositorySmokeTests
{
    [Fact]
    public async Task CommentRepository_CRUD_Smoke()
    {
        using var ctx = TestDbContextFactory.Create();
        var repo = new CommentRepository(ctx);
        var blog = new BlogPost { AuthorId = "u", Title = "t", Content = "c" };
        ctx.BlogPosts.Add(blog);
        await ctx.SaveChangesAsync();

        var created = await repo.CreateAsync(new Comment { BlogPostId = blog.Id, Content = "hi", AuthorId = "u", AuthorName = "User" });
        var fetched = await repo.GetByIdAsync(created.Id);
        fetched.Should().NotBeNull();

        created.Content = "updated";
        var updated = await repo.UpdateAsync(created);
        updated.Content.Should().Be("updated");

        (await repo.ExistsAsync(created.Id)).Should().BeTrue();
        await repo.DeleteAsync(created.Id);
        (await repo.ExistsAsync(created.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task CommentLikeRepository_CRUD_Smoke()
    {
        using var ctx = TestDbContextFactory.Create();
        var repo = new CommentLikeRepository(ctx);
        var blog = new BlogPost { AuthorId = "u", Title = "t", Content = "c" };
        var comment = new Comment { BlogPost = blog, BlogPostId = blog.Id, Content = "c", AuthorId = "u", AuthorName = "User" };
        ctx.BlogPosts.Add(blog);
        ctx.Comments.Add(comment);
        await ctx.SaveChangesAsync();

        var like = await repo.CreateAsync(new CommentLike { CommentId = comment.Id, UserId = "u", IsLike = true });
        (await repo.GetLikeCountAsync(comment.Id)).Should().Be(1);
        (await repo.GetDislikeCountAsync(comment.Id)).Should().Be(0);

        like.IsLike = false;
        await repo.UpdateAsync(like);
        (await repo.GetLikeCountAsync(comment.Id)).Should().Be(0);
        (await repo.GetDislikeCountAsync(comment.Id)).Should().Be(1);

        (await repo.ExistsAsync(like.Id)).Should().BeTrue();
        await repo.DeleteAsync(like.Id);
        (await repo.ExistsAsync(like.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task PostRatingRepository_CRUD_Smoke()
    {
        using var ctx = TestDbContextFactory.Create();
        var repo = new PostRatingRepository(ctx);
        var blog = new BlogPost { AuthorId = "u", Title = "t", Content = "c" };
        ctx.BlogPosts.Add(blog);
        await ctx.SaveChangesAsync();

        var r = await repo.CreateAsync(new PostRating { BlogPostId = blog.Id, UserId = "u", Rating = 2.5m });
        (await repo.GetAverageRatingAsync(blog.Id)).Should().Be(2.5m);
        (await repo.GetTotalRatingsAsync(blog.Id)).Should().Be(1);

        r.Rating = 3.0m;
        await repo.UpdateAsync(r);
        (await repo.GetAverageRatingAsync(blog.Id)).Should().Be(3.0m);

        (await repo.ExistsAsync(r.Id)).Should().BeTrue();
        await repo.DeleteAsync(r.Id);
        (await repo.ExistsAsync(r.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task SavedBlogRepository_CRUD_Smoke()
    {
        using var ctx = TestDbContextFactory.Create();
        var repo = new SavedBlogRepository(ctx);
        var blog = new BlogPost { AuthorId = "u", Title = "t", Content = "c" };
        ctx.BlogPosts.Add(blog);
        await ctx.SaveChangesAsync();

        var saved = await repo.SaveBlogAsync(new SavedBlog { UserId = "u", BlogPostId = blog.Id });
        (await repo.IsBookmarkedAsync("u", blog.Id)).Should().BeTrue();
        (await repo.GetSavedBlogAsync("u", blog.Id)).Should().NotBeNull();

        var list = (await repo.GetSavedBlogsByUserIdAsync("u")).ToList();
        list.Should().ContainSingle();

        await repo.DeleteSavedBlogAsync("u", blog.Id);
        (await repo.IsBookmarkedAsync("u", blog.Id)).Should().BeFalse();
    }
}


