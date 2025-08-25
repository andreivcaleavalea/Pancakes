using BlogService.Models.Entities;
using BlogService.Repositories.Implementations;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Repositories;

public class CommentLikeRepositoryTests : IDisposable
{
    private readonly BlogDbContext _context;
    private readonly CommentLikeRepository _repository;

    public CommentLikeRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new CommentLikeRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<Comment> CreateTestCommentAsync()
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

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = "Test comment",
            AuthorId = "user1",
            AuthorName = "Test User",
            BlogPostId = blogPost.Id
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    private async Task<CommentLike> CreateTestCommentLikeAsync(Guid commentId, string userId = "user1", bool isLike = true)
    {
        var commentLike = new CommentLike
        {
            Id = Guid.NewGuid(),
            CommentId = commentId,
            UserId = userId,
            IsLike = isLike
        };
        _context.CommentLikes.Add(commentLike);
        await _context.SaveChangesAsync();
        return commentLike;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLike_ReturnsLike()
    {
        var comment = await CreateTestCommentAsync();
        var like = await CreateTestCommentLikeAsync(comment.Id);

        var result = await _repository.GetByIdAsync(like.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(like.Id);
        result.CommentId.Should().Be(comment.Id);
        result.Comment.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentLike_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCommentAndUserAsync_ExistingLike_ReturnsLike()
    {
        var comment = await CreateTestCommentAsync();
        var like = await CreateTestCommentLikeAsync(comment.Id, "user123");

        var result = await _repository.GetByCommentAndUserAsync(comment.Id, "user123");

        result.Should().NotBeNull();
        result!.Id.Should().Be(like.Id);
        result.CommentId.Should().Be(comment.Id);
        result.UserId.Should().Be("user123");
    }

    [Fact]
    public async Task GetByCommentAndUserAsync_NonExistentLike_ReturnsNull()
    {
        var comment = await CreateTestCommentAsync();

        var result = await _repository.GetByCommentAndUserAsync(comment.Id, "nonexistent-user");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCommentIdAsync_MultipleLikes_ReturnsAllOrderedByDate()
    {
        var comment = await CreateTestCommentAsync();
        
        await Task.Delay(10);
        var like1 = await CreateTestCommentLikeAsync(comment.Id, "user1");
        
        await Task.Delay(10);
        var like2 = await CreateTestCommentLikeAsync(comment.Id, "user2");
        
        await Task.Delay(10);
        var like3 = await CreateTestCommentLikeAsync(comment.Id, "user3");

        var result = await _repository.GetByCommentIdAsync(comment.Id);

        var likes = result.ToList();
        likes.Should().HaveCount(3);
        
        // Should be ordered by CreatedAt descending (newest first)
        likes[0].CreatedAt.Should().BeAfter(likes[1].CreatedAt);
        likes[1].CreatedAt.Should().BeAfter(likes[2].CreatedAt);
    }

    [Fact]
    public async Task GetByCommentIdAsync_NoLikes_ReturnsEmpty()
    {
        var comment = await CreateTestCommentAsync();

        var result = await _repository.GetByCommentIdAsync(comment.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ValidLike_CreatesAndReturnsLike()
    {
        var comment = await CreateTestCommentAsync();
        var like = new CommentLike
        {
            CommentId = comment.Id,
            UserId = "user456",
            IsLike = true
        };

        var result = await _repository.CreateAsync(like);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.CommentId.Should().Be(comment.Id);
        result.UserId.Should().Be("user456");
        result.IsLike.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var dbLike = await _context.CommentLikes.FindAsync(result.Id);
        dbLike.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingLike_UpdatesLike()
    {
        var comment = await CreateTestCommentAsync();
        var like = await CreateTestCommentLikeAsync(comment.Id, "user1", true);
        var originalCreatedAt = like.CreatedAt;

        like.IsLike = false; // Change from like to dislike
        var result = await _repository.UpdateAsync(like);

        result.Should().NotBeNull();
        result.IsLike.Should().BeFalse();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CreatedAt.Should().Be(originalCreatedAt); // Should not change

        // Verify in database
        var dbLike = await _context.CommentLikes.FindAsync(like.Id);
        dbLike!.IsLike.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingLike_RemovesLike()
    {
        var comment = await CreateTestCommentAsync();
        var like = await CreateTestCommentLikeAsync(comment.Id);

        await _repository.DeleteAsync(like.Id);

        var dbLike = await _context.CommentLikes.FindAsync(like.Id);
        dbLike.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentLike_DoesNotThrow()
    {
        // Should not throw an exception
        await _repository.DeleteAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task ExistsAsync_ExistingLike_ReturnsTrue()
    {
        var comment = await CreateTestCommentAsync();
        var like = await CreateTestCommentLikeAsync(comment.Id);

        var result = await _repository.ExistsAsync(like.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentLike_ReturnsFalse()
    {
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetLikeCountAsync_MixedLikesAndDislikes_ReturnsOnlyLikeCount()
    {
        var comment = await CreateTestCommentAsync();
        
        // Create 3 likes and 2 dislikes
        await CreateTestCommentLikeAsync(comment.Id, "user1", true);
        await CreateTestCommentLikeAsync(comment.Id, "user2", true);
        await CreateTestCommentLikeAsync(comment.Id, "user3", true);
        await CreateTestCommentLikeAsync(comment.Id, "user4", false);
        await CreateTestCommentLikeAsync(comment.Id, "user5", false);

        var result = await _repository.GetLikeCountAsync(comment.Id);

        result.Should().Be(3);
    }

    [Fact]
    public async Task GetLikeCountAsync_NoLikes_ReturnsZero()
    {
        var comment = await CreateTestCommentAsync();

        var result = await _repository.GetLikeCountAsync(comment.Id);

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetDislikeCountAsync_MixedLikesAndDislikes_ReturnsOnlyDislikeCount()
    {
        var comment = await CreateTestCommentAsync();
        
        // Create 2 likes and 3 dislikes
        await CreateTestCommentLikeAsync(comment.Id, "user1", true);
        await CreateTestCommentLikeAsync(comment.Id, "user2", true);
        await CreateTestCommentLikeAsync(comment.Id, "user3", false);
        await CreateTestCommentLikeAsync(comment.Id, "user4", false);
        await CreateTestCommentLikeAsync(comment.Id, "user5", false);

        var result = await _repository.GetDislikeCountAsync(comment.Id);

        result.Should().Be(3);
    }

    [Fact]
    public async Task GetDislikeCountAsync_NoDislikes_ReturnsZero()
    {
        var comment = await CreateTestCommentAsync();

        var result = await _repository.GetDislikeCountAsync(comment.Id);

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetLikeAndDislikeCount_SameComment_BothWorkCorrectly()
    {
        var comment = await CreateTestCommentAsync();
        
        // Create 4 likes and 2 dislikes
        await CreateTestCommentLikeAsync(comment.Id, "user1", true);
        await CreateTestCommentLikeAsync(comment.Id, "user2", true);
        await CreateTestCommentLikeAsync(comment.Id, "user3", true);
        await CreateTestCommentLikeAsync(comment.Id, "user4", true);
        await CreateTestCommentLikeAsync(comment.Id, "user5", false);
        await CreateTestCommentLikeAsync(comment.Id, "user6", false);

        var likeCount = await _repository.GetLikeCountAsync(comment.Id);
        var dislikeCount = await _repository.GetDislikeCountAsync(comment.Id);

        likeCount.Should().Be(4);
        dislikeCount.Should().Be(2);
    }

    [Fact]
    public async Task CreateAndUpdateLike_UserChangesOpinion_WorksCorrectly()
    {
        var comment = await CreateTestCommentAsync();
        
        // User initially likes the comment
        var like = new CommentLike
        {
            CommentId = comment.Id,
            UserId = "changing-user",
            IsLike = true
        };
        var createdLike = await _repository.CreateAsync(like);

        // Verify initial like count
        var initialLikeCount = await _repository.GetLikeCountAsync(comment.Id);
        initialLikeCount.Should().Be(1);

        // User changes to dislike
        createdLike.IsLike = false;
        await _repository.UpdateAsync(createdLike);

        // Verify counts after change
        var finalLikeCount = await _repository.GetLikeCountAsync(comment.Id);
        var finalDislikeCount = await _repository.GetDislikeCountAsync(comment.Id);
        
        finalLikeCount.Should().Be(0);
        finalDislikeCount.Should().Be(1);
    }
}
