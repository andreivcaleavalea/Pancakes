using BlogService.Models.Entities;
using BlogService.Repositories.Implementations;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Repositories;

public class SavedBlogRepositoryTests : IDisposable
{
    private readonly BlogDbContext _context;
    private readonly SavedBlogRepository _repository;

    public SavedBlogRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new SavedBlogRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<BlogPost> CreateTestBlogPostAsync(string title = "Test Post")
    {
        var blogPost = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = "Test content",
            AuthorId = "author1",
            Status = PostStatus.Published
        };
        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();
        return blogPost;
    }

    private async Task<SavedBlog> CreateTestSavedBlogAsync(string userId, Guid blogPostId)
    {
        var savedBlog = new SavedBlog
        {
            UserId = userId,
            BlogPostId = blogPostId,
            SavedAt = DateTime.UtcNow
        };
        _context.SavedBlogs.Add(savedBlog);
        await _context.SaveChangesAsync();
        return savedBlog;
    }

    [Fact]
    public async Task GetSavedBlogsByUserIdAsync_UserWithSavedBlogs_ReturnsSavedBlogsOrderedByDate()
    {
        var blogPost1 = await CreateTestBlogPostAsync("Post 1");
        var blogPost2 = await CreateTestBlogPostAsync("Post 2");
        var blogPost3 = await CreateTestBlogPostAsync("Post 3");

        await Task.Delay(10);
        await CreateTestSavedBlogAsync("user1", blogPost1.Id);
        
        await Task.Delay(10);
        await CreateTestSavedBlogAsync("user1", blogPost2.Id);
        
        await Task.Delay(10);
        await CreateTestSavedBlogAsync("user1", blogPost3.Id);

        var result = await _repository.GetSavedBlogsByUserIdAsync("user1");

        var savedBlogs = result.ToList();
        savedBlogs.Should().HaveCount(3);
        
        // Should be ordered by SavedAt descending (newest first)
        savedBlogs[0].SavedAt.Should().BeAfter(savedBlogs[1].SavedAt);
        savedBlogs[1].SavedAt.Should().BeAfter(savedBlogs[2].SavedAt);
        
        // Should include blog post details
        savedBlogs.All(sb => sb.BlogPost != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetSavedBlogsByUserIdAsync_UserWithNoSavedBlogs_ReturnsEmpty()
    {
        var result = await _repository.GetSavedBlogsByUserIdAsync("user-with-no-saves");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSavedBlogAsync_ExistingSavedBlog_ReturnsSavedBlog()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var savedBlog = await CreateTestSavedBlogAsync("user1", blogPost.Id);

        var result = await _repository.GetSavedBlogAsync("user1", blogPost.Id);

        result.Should().NotBeNull();
        result!.UserId.Should().Be("user1");
        result.BlogPostId.Should().Be(blogPost.Id);
        result.BlogPost.Should().NotBeNull();
        result.BlogPost!.Title.Should().Be("Test Post");
    }

    [Fact]
    public async Task GetSavedBlogAsync_NonExistentSavedBlog_ReturnsNull()
    {
        var blogPost = await CreateTestBlogPostAsync();

        var result = await _repository.GetSavedBlogAsync("user1", blogPost.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveBlogAsync_NewSavedBlog_CreatesSavedBlog()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var savedBlog = new SavedBlog
        {
            UserId = "user123",
            BlogPostId = blogPost.Id,
            SavedAt = DateTime.UtcNow
        };

        var result = await _repository.SaveBlogAsync(savedBlog);

        result.Should().NotBeNull();
        result.UserId.Should().Be("user123");
        result.BlogPostId.Should().Be(blogPost.Id);
        result.SavedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.BlogPost.Should().NotBeNull();

        // Verify it was saved to database
        var dbSavedBlog = await _context.SavedBlogs
            .FirstOrDefaultAsync(sb => sb.UserId == "user123" && sb.BlogPostId == blogPost.Id);
        dbSavedBlog.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteSavedBlogAsync_ExistingSavedBlog_RemovesSavedBlog()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var savedBlog = await CreateTestSavedBlogAsync("user1", blogPost.Id);

        await _repository.DeleteSavedBlogAsync("user1", blogPost.Id);

        var dbSavedBlog = await _context.SavedBlogs
            .FirstOrDefaultAsync(sb => sb.UserId == "user1" && sb.BlogPostId == blogPost.Id);
        dbSavedBlog.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSavedBlogAsync_NonExistentSavedBlog_DoesNotThrow()
    {
        var blogPost = await CreateTestBlogPostAsync();

        // Should not throw an exception
        await _repository.DeleteSavedBlogAsync("user1", blogPost.Id);
    }

    [Fact]
    public async Task IsBookmarkedAsync_BookmarkedPost_ReturnsTrue()
    {
        var blogPost = await CreateTestBlogPostAsync();
        await CreateTestSavedBlogAsync("user1", blogPost.Id);

        var result = await _repository.IsBookmarkedAsync("user1", blogPost.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsBookmarkedAsync_NotBookmarkedPost_ReturnsFalse()
    {
        var blogPost = await CreateTestBlogPostAsync();

        var result = await _repository.IsBookmarkedAsync("user1", blogPost.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserSavedPostsAsync_MultipleSavedPosts_ReturnsAllWithBlogPosts()
    {
        var blogPost1 = await CreateTestBlogPostAsync("Post 1");
        var blogPost2 = await CreateTestBlogPostAsync("Post 2");
        var blogPost3 = await CreateTestBlogPostAsync("Post 3");

        await CreateTestSavedBlogAsync("user1", blogPost1.Id);
        await CreateTestSavedBlogAsync("user1", blogPost2.Id);
        await CreateTestSavedBlogAsync("user2", blogPost3.Id); // Different user

        var result = await _repository.GetUserSavedPostsAsync("user1");

        var savedBlogs = result.ToList();
        savedBlogs.Should().HaveCount(2);
        savedBlogs.All(sb => sb.UserId == "user1").Should().BeTrue();
        savedBlogs.All(sb => sb.BlogPost != null).Should().BeTrue();
        
        var blogPostTitles = savedBlogs.Select(sb => sb.BlogPost!.Title).ToList();
        blogPostTitles.Should().Contain("Post 1");
        blogPostTitles.Should().Contain("Post 2");
        blogPostTitles.Should().NotContain("Post 3");
    }

    [Fact]
    public async Task GetUserSavedPostsAsync_NoSavedPosts_ReturnsEmpty()
    {
        var result = await _repository.GetUserSavedPostsAsync("user-with-no-saves");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPostSavesByUsersAsync_MultipleUsers_ReturnsFilteredSaves()
    {
        var blogPost = await CreateTestBlogPostAsync();

        await CreateTestSavedBlogAsync("user1", blogPost.Id);
        await CreateTestSavedBlogAsync("user2", blogPost.Id);
        await CreateTestSavedBlogAsync("user3", blogPost.Id);
        await CreateTestSavedBlogAsync("user4", blogPost.Id);

        var targetUsers = new[] { "user1", "user3" };
        var result = await _repository.GetPostSavesByUsersAsync(blogPost.Id, targetUsers);

        var saves = result.ToList();
        saves.Should().HaveCount(2);
        saves.Should().Contain(sb => sb.UserId == "user1");
        saves.Should().Contain(sb => sb.UserId == "user3");
        saves.Should().NotContain(sb => sb.UserId == "user2");
        saves.Should().NotContain(sb => sb.UserId == "user4");
        saves.All(sb => sb.BlogPost != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetPostSavesByUsersAsync_NoMatchingUsers_ReturnsEmpty()
    {
        var blogPost = await CreateTestBlogPostAsync();
        await CreateTestSavedBlogAsync("user1", blogPost.Id);

        var targetUsers = new[] { "user2", "user3" };
        var result = await _repository.GetPostSavesByUsersAsync(blogPost.Id, targetUsers);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPostSavesByUsersAsync_EmptyUserList_ReturnsEmpty()
    {
        var blogPost = await CreateTestBlogPostAsync();
        await CreateTestSavedBlogAsync("user1", blogPost.Id);

        var targetUsers = new string[0];
        var result = await _repository.GetPostSavesByUsersAsync(blogPost.Id, targetUsers);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteSaveLifecycle_SaveAndUnsave_WorksCorrectly()
    {
        var blogPost = await CreateTestBlogPostAsync();
        
        // Initially not bookmarked
        var isBookmarked1 = await _repository.IsBookmarkedAsync("lifecycle-user", blogPost.Id);
        isBookmarked1.Should().BeFalse();

        // Save the blog
        var savedBlog = new SavedBlog
        {
            UserId = "lifecycle-user",
            BlogPostId = blogPost.Id,
            SavedAt = DateTime.UtcNow
        };
        await _repository.SaveBlogAsync(savedBlog);

        // Verify it's now bookmarked
        var isBookmarked2 = await _repository.IsBookmarkedAsync("lifecycle-user", blogPost.Id);
        isBookmarked2.Should().BeTrue();

        // Verify it appears in user's saved blogs
        var userSaves = await _repository.GetUserSavedPostsAsync("lifecycle-user");
        userSaves.Should().HaveCount(1);

        // Unsave the blog
        await _repository.DeleteSavedBlogAsync("lifecycle-user", blogPost.Id);

        // Verify it's no longer bookmarked
        var isBookmarked3 = await _repository.IsBookmarkedAsync("lifecycle-user", blogPost.Id);
        isBookmarked3.Should().BeFalse();

        // Verify it no longer appears in user's saved blogs
        var userSaves2 = await _repository.GetUserSavedPostsAsync("lifecycle-user");
        userSaves2.Should().BeEmpty();
    }

    [Fact]
    public async Task MultipleSaveOperations_SameUserDifferentPosts_AllWork()
    {
        var blogPost1 = await CreateTestBlogPostAsync("Post 1");
        var blogPost2 = await CreateTestBlogPostAsync("Post 2");
        var blogPost3 = await CreateTestBlogPostAsync("Post 3");

        // Save multiple posts for the same user
        await _repository.SaveBlogAsync(new SavedBlog { UserId = "multi-user", BlogPostId = blogPost1.Id, SavedAt = DateTime.UtcNow });
        await _repository.SaveBlogAsync(new SavedBlog { UserId = "multi-user", BlogPostId = blogPost2.Id, SavedAt = DateTime.UtcNow });
        await _repository.SaveBlogAsync(new SavedBlog { UserId = "multi-user", BlogPostId = blogPost3.Id, SavedAt = DateTime.UtcNow });

        // Verify all are saved
        var isBookmarked1 = await _repository.IsBookmarkedAsync("multi-user", blogPost1.Id);
        var isBookmarked2 = await _repository.IsBookmarkedAsync("multi-user", blogPost2.Id);
        var isBookmarked3 = await _repository.IsBookmarkedAsync("multi-user", blogPost3.Id);

        isBookmarked1.Should().BeTrue();
        isBookmarked2.Should().BeTrue();
        isBookmarked3.Should().BeTrue();

        // Verify count in user saves
        var userSaves = await _repository.GetUserSavedPostsAsync("multi-user");
        userSaves.Should().HaveCount(3);

        // Remove one save
        await _repository.DeleteSavedBlogAsync("multi-user", blogPost2.Id);

        // Verify specific removal
        var isStillBookmarked2 = await _repository.IsBookmarkedAsync("multi-user", blogPost2.Id);
        isStillBookmarked2.Should().BeFalse();

        // Verify others still saved
        var userSaves2 = await _repository.GetUserSavedPostsAsync("multi-user");
        userSaves2.Should().HaveCount(2);
    }
}
