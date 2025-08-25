using BlogService.Models.Entities;
using BlogService.Repositories.Implementations;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Repositories;

public class CommentRepositoryTests : IDisposable
{
    private readonly BlogDbContext _context;
    private readonly CommentRepository _repository;

    public CommentRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new CommentRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<BlogPost> CreateTestBlogPostAsync()
    {
        var blogPost = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = "Test Post",
            Content = "Test content",
            AuthorId = "author1",
            Status = PostStatus.Published
        };
        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();
        return blogPost;
    }

    private async Task<Comment> CreateTestCommentAsync(Guid blogPostId, Guid? parentCommentId = null)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = "Test comment",
            AuthorId = "user1",
            AuthorName = "Test User",
            BlogPostId = blogPostId,
            ParentCommentId = parentCommentId
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingComment_ReturnsComment()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var comment = await CreateTestCommentAsync(blogPost.Id);

        var result = await _repository.GetByIdAsync(comment.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(comment.Id);
        result.Content.Should().Be("Test comment");
        result.BlogPost.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentComment_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBlogPostIdAsync_ReturnsTopLevelCommentsWithReplies()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var parentComment = await CreateTestCommentAsync(blogPost.Id);
        var reply1 = await CreateTestCommentAsync(blogPost.Id, parentComment.Id);
        var reply2 = await CreateTestCommentAsync(blogPost.Id, reply1.Id);
        var anotherParent = await CreateTestCommentAsync(blogPost.Id);

        var result = await _repository.GetByBlogPostIdAsync(blogPost.Id);

        result.Should().HaveCount(2); // Only top-level comments
        var parentResults = result.ToList();
        
        // Should include replies
        var firstParent = parentResults.First(c => c.Id == parentComment.Id);
        firstParent.Replies.Should().HaveCount(1);
        firstParent.Replies.First().Replies.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByBlogPostIdAsync_EmptyBlogPost_ReturnsEmpty()
    {
        var blogPost = await CreateTestBlogPostAsync();

        var result = await _repository.GetByBlogPostIdAsync(blogPost.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ValidComment_CreatesAndReturnsComment()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var comment = new Comment
        {
            Content = "New comment",
            AuthorId = "user2",
            AuthorName = "User Two",
            BlogPostId = blogPost.Id
        };

        var result = await _repository.CreateAsync(comment);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Content.Should().Be("New comment");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var dbComment = await _context.Comments.FindAsync(result.Id);
        dbComment.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingComment_UpdatesComment()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var comment = await CreateTestCommentAsync(blogPost.Id);
        var originalCreatedAt = comment.CreatedAt;

        comment.Content = "Updated content";
        var result = await _repository.UpdateAsync(comment);

        result.Should().NotBeNull();
        result.Content.Should().Be("Updated content");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CreatedAt.Should().Be(originalCreatedAt); // Should not change

        // Verify in database
        var dbComment = await _context.Comments.FindAsync(comment.Id);
        dbComment!.Content.Should().Be("Updated content");
    }

    [Fact]
    public async Task DeleteAsync_ExistingComment_RemovesComment()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var comment = await CreateTestCommentAsync(blogPost.Id);

        await _repository.DeleteAsync(comment.Id);

        var dbComment = await _context.Comments.FindAsync(comment.Id);
        dbComment.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentComment_DoesNotThrow()
    {
        // Should not throw an exception
        await _repository.DeleteAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task ExistsAsync_ExistingComment_ReturnsTrue()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var comment = await CreateTestCommentAsync(blogPost.Id);

        var result = await _repository.ExistsAsync(comment.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentComment_ReturnsFalse()
    {
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasRepliesAsync_CommentWithReplies_ReturnsTrue()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var parentComment = await CreateTestCommentAsync(blogPost.Id);
        await CreateTestCommentAsync(blogPost.Id, parentComment.Id);

        var result = await _repository.HasRepliesAsync(parentComment.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasRepliesAsync_CommentWithoutReplies_ReturnsFalse()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var comment = await CreateTestCommentAsync(blogPost.Id);

        var result = await _repository.HasRepliesAsync(comment.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdWithRepliesAsync_CommentWithNestedReplies_ReturnsFullHierarchy()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var parentComment = await CreateTestCommentAsync(blogPost.Id);
        var reply1 = await CreateTestCommentAsync(blogPost.Id, parentComment.Id);
        var reply2 = await CreateTestCommentAsync(blogPost.Id, reply1.Id);
        var reply3 = await CreateTestCommentAsync(blogPost.Id, reply2.Id);

        var result = await _repository.GetByIdWithRepliesAsync(parentComment.Id);

        result.Should().NotBeNull();
        result!.Replies.Should().HaveCount(1);
        result.Replies.First().Replies.Should().HaveCount(1);
        result.Replies.First().Replies.First().Replies.Should().HaveCount(1);
        result.BlogPost.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdWithRepliesAsync_NonExistentComment_ReturnsNull()
    {
        var result = await _repository.GetByIdWithRepliesAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBlogPostIdAsync_CommentsOrderedCorrectly()
    {
        var blogPost = await CreateTestBlogPostAsync();
        
        // Create comments with different timestamps
        await Task.Delay(10); // Small delay to ensure different timestamps
        var comment1 = await CreateTestCommentAsync(blogPost.Id);
        
        await Task.Delay(10);
        var comment2 = await CreateTestCommentAsync(blogPost.Id);
        
        await Task.Delay(10);
        var comment3 = await CreateTestCommentAsync(blogPost.Id);

        var result = await _repository.GetByBlogPostIdAsync(blogPost.Id);

        var comments = result.ToList();
        comments.Should().HaveCount(3);
        
        // Should be ordered by CreatedAt descending (newest first)
        comments[0].CreatedAt.Should().BeAfter(comments[1].CreatedAt);
        comments[1].CreatedAt.Should().BeAfter(comments[2].CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_CommentWithReply_LoadsRepliesCorrectly()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var parentComment = await CreateTestCommentAsync(blogPost.Id);
        
        var replyComment = new Comment
        {
            Content = "Reply comment",
            AuthorId = "user2",
            AuthorName = "User Two",
            BlogPostId = blogPost.Id,
            ParentCommentId = parentComment.Id
        };

        var result = await _repository.CreateAsync(replyComment);

        result.Should().NotBeNull();
        result.ParentCommentId.Should().Be(parentComment.Id);

        // Verify the parent comment now shows it has replies
        var parentWithReplies = await _repository.GetByIdWithRepliesAsync(parentComment.Id);
        parentWithReplies!.Replies.Should().HaveCount(1);
        parentWithReplies.Replies.First().Id.Should().Be(result.Id);
    }
}
