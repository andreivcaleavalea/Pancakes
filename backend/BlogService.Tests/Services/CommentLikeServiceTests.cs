using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using BlogService.Tests.TestUtilities;

namespace BlogService.Tests.Services;

public class CommentLikeServiceTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public CommentLikeServiceTests(MappingFixture mapping)
    {
        _mapper = mapping.Mapper;
    }

    private static CommentLikeService CreateService(
        IMapper mapper,
        out Mock<ICommentLikeRepository> likeRepo,
        out Mock<ICommentRepository> commentRepo,
        out Mock<IAuthorizationService> auth,
        out Mock<IModelValidationService> validator)
    {
        likeRepo = new Mock<ICommentLikeRepository>(MockBehavior.Strict);
        commentRepo = new Mock<ICommentRepository>(MockBehavior.Strict);
        auth = new Mock<IAuthorizationService>(MockBehavior.Loose);
        validator = new Mock<IModelValidationService>(MockBehavior.Loose);
        return new CommentLikeService(likeRepo.Object, commentRepo.Object, mapper, auth.Object, validator.Object);
    }

    [Fact]
    public async Task Create_When_None_Exists_Creates_New()
    {
        var service = CreateService(_mapper, out var likeRepo, out var commentRepo, out var auth, out var validator);
        var commentId = Guid.NewGuid();
        commentRepo.Setup(c => c.ExistsAsync(commentId)).ReturnsAsync(true);
        likeRepo.Setup(r => r.GetByCommentAndUserAsync(commentId, "u")).ReturnsAsync((CommentLike?)null);
        likeRepo.Setup(r => r.CreateAsync(It.IsAny<CommentLike>())).ReturnsAsync((CommentLike cl) => cl);

        var dto = new CreateCommentLikeDto { CommentId = commentId, UserId = "u", IsLike = true };
        var result = await service.CreateOrUpdateLikeAsync(dto);
        result.IsLike.Should().BeTrue();
    }

    [Fact]
    public async Task Update_When_Exists_Updates_IsLike()
    {
        var service = CreateService(_mapper, out var likeRepo, out var commentRepo, out var auth, out var validator);
        var commentId = Guid.NewGuid();
        commentRepo.Setup(c => c.ExistsAsync(commentId)).ReturnsAsync(true);

        var existing = new CommentLike { Id = Guid.NewGuid(), CommentId = commentId, UserId = "u", IsLike = false };
        likeRepo.Setup(r => r.GetByCommentAndUserAsync(commentId, "u")).ReturnsAsync(existing);
        likeRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync((CommentLike cl) => cl);

        var dto = new CreateCommentLikeDto { CommentId = commentId, UserId = "u", IsLike = true };
        var result = await service.CreateOrUpdateLikeAsync(dto);
        result.IsLike.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Missing_Throws()
    {
        var service = CreateService(_mapper, out var likeRepo, out var commentRepo, out var auth, out var validator);
        var commentId = Guid.NewGuid();
        likeRepo.Setup(r => r.GetByCommentAndUserAsync(commentId, "u")).ReturnsAsync((CommentLike?)null);

        Func<Task> act = async () => await service.DeleteLikeAsync(commentId, "u");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Stats_Returns_Counts_And_UserLike_When_User_Provided()
    {
        var service = CreateService(_mapper, out var likeRepo, out var commentRepo, out var auth, out var validator);
        var commentId = Guid.NewGuid();
        likeRepo.Setup(r => r.GetLikeCountAsync(commentId)).ReturnsAsync(7);
        likeRepo.Setup(r => r.GetDislikeCountAsync(commentId)).ReturnsAsync(3);
        likeRepo.Setup(r => r.GetByCommentAndUserAsync(commentId, "u")).ReturnsAsync(new CommentLike { IsLike = true });

        var statsWithUser = await service.GetLikeStatsAsync(commentId, "u");
        statsWithUser.LikeCount.Should().Be(7);
        statsWithUser.DislikeCount.Should().Be(3);
        statsWithUser.UserLike.Should().BeTrue();

        likeRepo.Setup(r => r.GetByCommentAndUserAsync(commentId, "u")).ReturnsAsync((CommentLike?)null);
        var statsNoUserVote = await service.GetLikeStatsAsync(commentId, "u");
        statsNoUserVote.UserLike.Should().BeNull();

        var statsAnon = await service.GetLikeStatsAsync(commentId, (string?)null);
        statsAnon.UserLike.Should().BeNull();
    }
}


