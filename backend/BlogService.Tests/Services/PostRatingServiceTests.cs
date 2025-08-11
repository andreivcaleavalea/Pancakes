using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Services;

public class PostRatingServiceTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public PostRatingServiceTests(MappingFixture mapping)
    {
        _mapper = mapping.Mapper;
    }

    private static PostRatingService CreateService(
        IMapper mapper,
        out Mock<IPostRatingRepository> ratingRepo,
        out Mock<IBlogPostRepository> postRepo,
        out Mock<IAuthorizationService> auth,
        out Mock<IModelValidationService> validator)
    {
        ratingRepo = new Mock<IPostRatingRepository>(MockBehavior.Strict);
        postRepo = new Mock<IBlogPostRepository>(MockBehavior.Strict);
        auth = new Mock<IAuthorizationService>(MockBehavior.Loose);
        validator = new Mock<IModelValidationService>(MockBehavior.Loose);
        var logger = new Mock<ILogger<PostRatingService>>();

        return new PostRatingService(ratingRepo.Object, postRepo.Object, mapper, auth.Object, validator.Object, logger.Object);
    }

    [Theory]
    [InlineData(0.4)]
    [InlineData(5.5)]
    public async Task CreateOrUpdate_Invalid_Rating_Throws(decimal invalid)
    {
        var service = CreateService(_mapper, out var ratingRepo, out var postRepo, out var auth, out var validator);
        var postId = Guid.NewGuid();
        postRepo.Setup(p => p.ExistsAsync(postId)).ReturnsAsync(true);

        var dto = new CreatePostRatingDto { BlogPostId = postId, UserId = "u", Rating = invalid };
        Func<Task> act = async () => await service.CreateOrUpdateRatingAsync(dto);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Rating must be between 0.5 and 5.0");
    }

    [Fact]
    public async Task CreateOrUpdate_Rounds_To_Nearest_Half()
    {
        var service = CreateService(_mapper, out var ratingRepo, out var postRepo, out var auth, out var validator);
        var postId = Guid.NewGuid();
        postRepo.Setup(p => p.ExistsAsync(postId)).ReturnsAsync(true);

        // No existing rating => create
        ratingRepo.Setup(r => r.GetByBlogPostAndUserAsync(postId, "u")).ReturnsAsync((PostRating?)null);
        ratingRepo.Setup(r => r.CreateAsync(It.IsAny<PostRating>()))
            .ReturnsAsync((PostRating pr) => pr);

        var dto = new CreatePostRatingDto { BlogPostId = postId, UserId = "u", Rating = 2.26m };
        var result = await service.CreateOrUpdateRatingAsync(dto);
        result.Rating.Should().Be(2.5m);
    }

    [Fact]
    public async Task CreateOrUpdate_Upserts_Update_When_Exists()
    {
        var service = CreateService(_mapper, out var ratingRepo, out var postRepo, out var auth, out var validator);
        var postId = Guid.NewGuid();
        postRepo.Setup(p => p.ExistsAsync(postId)).ReturnsAsync(true);

        var existing = new PostRating { Id = Guid.NewGuid(), BlogPostId = postId, UserId = "u", Rating = 1.0m };
        ratingRepo.Setup(r => r.GetByBlogPostAndUserAsync(postId, "u")).ReturnsAsync(existing);
        ratingRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync((PostRating pr) => pr);

        var dto = new CreatePostRatingDto { BlogPostId = postId, UserId = "u", Rating = 3.74m };
        var result = await service.CreateOrUpdateRatingAsync(dto);
        result.Rating.Should().Be(3.5m);
    }

    [Fact]
    public async Task Delete_NotFound_Throws()
    {
        var service = CreateService(_mapper, out var ratingRepo, out var postRepo, out var auth, out var validator);
        var postId = Guid.NewGuid();
        ratingRepo.Setup(r => r.GetByBlogPostAndUserAsync(postId, "u")).ReturnsAsync((PostRating?)null);

        Func<Task> act = async () => await service.DeleteRatingAsync(postId, "u");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Stats_Aggregates_And_Includes_UserRating()
    {
        var service = CreateService(_mapper, out var ratingRepo, out var postRepo, out var auth, out var validator);
        var postId = Guid.NewGuid();

        ratingRepo.Setup(r => r.GetAverageRatingAsync(postId)).ReturnsAsync(3.333m);
        ratingRepo.Setup(r => r.GetTotalRatingsAsync(postId)).ReturnsAsync(6);
        ratingRepo.Setup(r => r.GetRatingDistributionAsync(postId)).ReturnsAsync(new Dictionary<decimal, int>
        {
            [0.5m] = 1,
            [1.0m] = 0,
            [1.5m] = 1,
            [2.0m] = 1,
            [2.5m] = 1,
            [3.0m] = 1,
        });

        ratingRepo.Setup(r => r.GetByBlogPostAndUserAsync(postId, "u")).ReturnsAsync(new PostRating { Rating = 2.5m });

        var withUser = await service.GetRatingStatsAsync(postId, "u");
        withUser.AverageRating.Should().Be(3.3m);
        withUser.TotalRatings.Should().Be(6);
        withUser.UserRating.Should().Be(2.5m);
        withUser.RatingDistribution.Should().ContainKey(0.5m);

        ratingRepo.Setup(r => r.GetByBlogPostAndUserAsync(postId, "u")).ReturnsAsync((PostRating?)null);
        var withoutUser = await service.GetRatingStatsAsync(postId, "u");
        withoutUser.UserRating.Should().BeNull();
    }
}


