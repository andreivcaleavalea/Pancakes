using BlogService.Models.Entities;
using BlogService.Repositories.Implementations;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Repositories;

public class PostRatingRepositoryTests : IDisposable
{
    private readonly BlogDbContext _context;
    private readonly PostRatingRepository _repository;

    public PostRatingRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new PostRatingRepository(_context);
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

    private async Task<PostRating> CreateTestRatingAsync(Guid blogPostId, string userId = "user1", decimal rating = 4.0m)
    {
        var postRating = new PostRating
        {
            Id = Guid.NewGuid(),
            BlogPostId = blogPostId,
            UserId = userId,
            Rating = rating
        };
        _context.PostRatings.Add(postRating);
        await _context.SaveChangesAsync();
        return postRating;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRating_ReturnsRating()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var rating = await CreateTestRatingAsync(blogPost.Id);

        var result = await _repository.GetByIdAsync(rating.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(rating.Id);
        result.BlogPostId.Should().Be(blogPost.Id);
        result.BlogPost.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentRating_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBlogPostAndUserAsync_ExistingRating_ReturnsRating()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var rating = await CreateTestRatingAsync(blogPost.Id, "user123", 3.5m);

        var result = await _repository.GetByBlogPostAndUserAsync(blogPost.Id, "user123");

        result.Should().NotBeNull();
        result!.Id.Should().Be(rating.Id);
        result.BlogPostId.Should().Be(blogPost.Id);
        result.UserId.Should().Be("user123");
        result.Rating.Should().Be(3.5m);
    }

    [Fact]
    public async Task GetByBlogPostAndUserAsync_NonExistentRating_ReturnsNull()
    {
        var blogPost = await CreateTestBlogPostAsync();

        var result = await _repository.GetByBlogPostAndUserAsync(blogPost.Id, "nonexistent-user");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBlogPostIdAsync_MultipleRatings_ReturnsAllOrderedByDate()
    {
        var blogPost = await CreateTestBlogPostAsync();
        
        await Task.Delay(10);
        var rating1 = await CreateTestRatingAsync(blogPost.Id, "user1", 5.0m);
        
        await Task.Delay(10);
        var rating2 = await CreateTestRatingAsync(blogPost.Id, "user2", 3.0m);
        
        await Task.Delay(10);
        var rating3 = await CreateTestRatingAsync(blogPost.Id, "user3", 4.0m);

        var result = await _repository.GetByBlogPostIdAsync(blogPost.Id);

        var ratings = result.ToList();
        ratings.Should().HaveCount(3);
        
        // Should be ordered by CreatedAt descending (newest first)
        ratings[0].CreatedAt.Should().BeAfter(ratings[1].CreatedAt);
        ratings[1].CreatedAt.Should().BeAfter(ratings[2].CreatedAt);
    }

    [Fact]
    public async Task GetByBlogPostIdAsync_NoRatings_ReturnsEmpty()
    {
        var blogPost = await CreateTestBlogPostAsync();

        var result = await _repository.GetByBlogPostIdAsync(blogPost.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ValidRating_CreatesAndReturnsRating()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var rating = new PostRating
        {
            BlogPostId = blogPost.Id,
            UserId = "user456",
            Rating = 4.5m
        };

        var result = await _repository.CreateAsync(rating);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.BlogPostId.Should().Be(blogPost.Id);
        result.UserId.Should().Be("user456");
        result.Rating.Should().Be(4.5m);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var dbRating = await _context.PostRatings.FindAsync(result.Id);
        dbRating.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingRating_UpdatesRating()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var rating = await CreateTestRatingAsync(blogPost.Id, "user1", 3.0m);
        var originalCreatedAt = rating.CreatedAt;

        rating.Rating = 5.0m; // Change rating
        var result = await _repository.UpdateAsync(rating);

        result.Should().NotBeNull();
        result.Rating.Should().Be(5.0m);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CreatedAt.Should().Be(originalCreatedAt); // Should not change

        // Verify in database
        var dbRating = await _context.PostRatings.FindAsync(rating.Id);
        dbRating!.Rating.Should().Be(5.0m);
    }

    [Fact]
    public async Task DeleteAsync_ExistingRating_RemovesRating()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var rating = await CreateTestRatingAsync(blogPost.Id);

        await _repository.DeleteAsync(rating.Id);

        var dbRating = await _context.PostRatings.FindAsync(rating.Id);
        dbRating.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentRating_DoesNotThrow()
    {
        // Should not throw an exception
        await _repository.DeleteAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task ExistsAsync_ExistingRating_ReturnsTrue()
    {
        var blogPost = await CreateTestBlogPostAsync();
        var rating = await CreateTestRatingAsync(blogPost.Id);

        var result = await _repository.ExistsAsync(rating.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentRating_ReturnsFalse()
    {
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAverageRatingAsync_MultipleRatings_ReturnsCorrectAverage()
    {
        var blogPost = await CreateTestBlogPostAsync();
        
        // Create ratings: 2.0, 3.0, 4.0, 5.0 (average = 3.5)
        await CreateTestRatingAsync(blogPost.Id, "user1", 2.0m);
        await CreateTestRatingAsync(blogPost.Id, "user2", 3.0m);
        await CreateTestRatingAsync(blogPost.Id, "user3", 4.0m);
        await CreateTestRatingAsync(blogPost.Id, "user4", 5.0m);

        var result = await _repository.GetAverageRatingAsync(blogPost.Id);

        result.Should().Be(3.5m);
    }

    [Fact]
    public async Task GetAverageRatingAsync_NoRatings_ReturnsZero()
    {
        var blogPost = await CreateTestBlogPostAsync();

        var result = await _repository.GetAverageRatingAsync(blogPost.Id);

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetTotalRatingsAsync_MultipleRatings_ReturnsCorrectCount()
    {
        var blogPost = await CreateTestBlogPostAsync();
        
        await CreateTestRatingAsync(blogPost.Id, "user1", 5.0m);
        await CreateTestRatingAsync(blogPost.Id, "user2", 4.0m);
        await CreateTestRatingAsync(blogPost.Id, "user3", 3.0m);

        var result = await _repository.GetTotalRatingsAsync(blogPost.Id);

        result.Should().Be(3);
    }

    [Fact]
    public async Task GetTotalRatingsAsync_NoRatings_ReturnsZero()
    {
        var blogPost = await CreateTestBlogPostAsync();

        var result = await _repository.GetTotalRatingsAsync(blogPost.Id);

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetRatingDistributionAsync_VariousRatings_ReturnsCorrectDistribution()
    {
        var blogPost = await CreateTestBlogPostAsync();
        
        // Create distribution: 1.0x1, 3.0x2, 4.0x1, 5.0x3
        await CreateTestRatingAsync(blogPost.Id, "user1", 1.0m);
        await CreateTestRatingAsync(blogPost.Id, "user2", 3.0m);
        await CreateTestRatingAsync(blogPost.Id, "user3", 3.0m);
        await CreateTestRatingAsync(blogPost.Id, "user4", 4.0m);
        await CreateTestRatingAsync(blogPost.Id, "user5", 5.0m);
        await CreateTestRatingAsync(blogPost.Id, "user6", 5.0m);
        await CreateTestRatingAsync(blogPost.Id, "user7", 5.0m);

        var result = await _repository.GetRatingDistributionAsync(blogPost.Id);

        result.Should().HaveCount(4);
        result[1.0m].Should().Be(1);
        result[3.0m].Should().Be(2);
        result[4.0m].Should().Be(1);
        result[5.0m].Should().Be(3);
    }

    [Fact]
    public async Task GetRatingDistributionAsync_NoRatings_ReturnsEmptyDictionary()
    {
        var blogPost = await CreateTestBlogPostAsync();

        var result = await _repository.GetRatingDistributionAsync(blogPost.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserRatingsAsync_MultipleRatings_ReturnsUserRatingsWithBlogPosts()
    {
        var blogPost1 = await CreateTestBlogPostAsync();
        var blogPost2 = await CreateTestBlogPostAsync();
        
        await CreateTestRatingAsync(blogPost1.Id, "target-user", 4.0m);
        await CreateTestRatingAsync(blogPost2.Id, "target-user", 5.0m);
        await CreateTestRatingAsync(blogPost1.Id, "other-user", 3.0m); // Different user

        var result = await _repository.GetUserRatingsAsync("target-user");

        var ratings = result.ToList();
        ratings.Should().HaveCount(2);
        ratings.All(r => r.UserId == "target-user").Should().BeTrue();
        ratings.All(r => r.BlogPost != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetUserRatingsAsync_NoRatings_ReturnsEmpty()
    {
        var result = await _repository.GetUserRatingsAsync("nonexistent-user");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPostRatingsByUsersAsync_MultipleUsers_ReturnsFilteredRatings()
    {
        var blogPost = await CreateTestBlogPostAsync();
        
        await CreateTestRatingAsync(blogPost.Id, "user1", 4.0m);
        await CreateTestRatingAsync(blogPost.Id, "user2", 5.0m);
        await CreateTestRatingAsync(blogPost.Id, "user3", 3.0m);
        await CreateTestRatingAsync(blogPost.Id, "user4", 2.0m);

        var targetUsers = new[] { "user1", "user3" };
        var result = await _repository.GetPostRatingsByUsersAsync(blogPost.Id, targetUsers);

        var ratings = result.ToList();
        ratings.Should().HaveCount(2);
        ratings.Should().Contain(r => r.UserId == "user1" && r.Rating == 4.0m);
        ratings.Should().Contain(r => r.UserId == "user3" && r.Rating == 3.0m);
        ratings.All(r => r.BlogPost != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetPostRatingsByUsersAsync_NoMatchingUsers_ReturnsEmpty()
    {
        var blogPost = await CreateTestBlogPostAsync();
        await CreateTestRatingAsync(blogPost.Id, "user1", 4.0m);

        var targetUsers = new[] { "user2", "user3" };
        var result = await _repository.GetPostRatingsByUsersAsync(blogPost.Id, targetUsers);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteRatingLifecycle_CreateUpdateDelete_WorksCorrectly()
    {
        var blogPost = await CreateTestBlogPostAsync();
        
        // Create rating
        var rating = new PostRating
        {
            BlogPostId = blogPost.Id,
            UserId = "lifecycle-user",
            Rating = 3.0m
        };
        var created = await _repository.CreateAsync(rating);
        
        // Verify creation
        var average1 = await _repository.GetAverageRatingAsync(blogPost.Id);
        average1.Should().Be(3.0m);
        
        // Update rating
        created.Rating = 5.0m;
        await _repository.UpdateAsync(created);
        
        // Verify update
        var average2 = await _repository.GetAverageRatingAsync(blogPost.Id);
        average2.Should().Be(5.0m);
        
        // Delete rating
        await _repository.DeleteAsync(created.Id);
        
        // Verify deletion
        var average3 = await _repository.GetAverageRatingAsync(blogPost.Id);
        average3.Should().Be(0);
        
        var exists = await _repository.ExistsAsync(created.Id);
        exists.Should().BeFalse();
    }
}
