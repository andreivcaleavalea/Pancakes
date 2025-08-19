using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Services.Implementations;
using UserService.Repositories.Interfaces;
using UserService.Models.Entities;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;
using UserService.Helpers;

namespace UserService.Tests.Services;

public class FriendshipServiceTests
{
    private static FriendshipService CreateService(out Mock<IFriendshipRepository> repo, out Mock<IUserRepository> users, out Mock<ICurrentUserService> current)
    {
        repo = new Mock<IFriendshipRepository>(MockBehavior.Strict);
        users = new Mock<IUserRepository>(MockBehavior.Strict);
        current = new Mock<ICurrentUserService>(MockBehavior.Strict);
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FriendshipService>>().Object;
        return new FriendshipService(repo.Object, users.Object, mapper, current.Object, logger);
    }

    [Fact]
    public async Task GetUserFriendsAsync_Should_ReturnFriendDtoList()
    {
        // Arrange
        var s = CreateService(out var repo, out var users, out var current);
        repo.Setup(r => r.GetUserFriendsAsync("me")).ReturnsAsync(new List<Friendship>
        {
            new Friendship { SenderId = "me", ReceiverId = "u2", Status = FriendshipStatus.Accepted, CreatedAt = DateTime.UtcNow }
        });
        users.Setup(u => u.GetByIdAsync("u2")).ReturnsAsync(new User { Id = "u2", Name = "B" });
        // Act
        var res = (await s.GetUserFriendsAsync("me")).ToList();
        // Assert
        res.Should().HaveCount(1);
        res[0].UserId.Should().Be("u2");
    }

    [Fact]
    public async Task SendFriendRequest_Accept_Reject_Should_Work()
    {
        // Arrange
        var s = CreateService(out var repo, out var users, out var current);
        users.Setup(u => u.GetByIdAsync("u2")).ReturnsAsync(new User { Id = "u2" });
        repo.Setup(r => r.GetFriendshipAsync("u1", "u2")).ReturnsAsync((Friendship?)null);
        repo.Setup(r => r.CreateAsync(It.IsAny<Friendship>())).ReturnsAsync((Friendship f) => f);
        // Act
        var sent = await s.SendFriendRequestAsync("u1", "u2");
        // Assert
        sent.Status.Should().Be("Pending");

        var pending = new Friendship { Id = Guid.NewGuid(), ReceiverId = "u2", Status = FriendshipStatus.Pending };
        repo.Setup(r => r.GetByIdAsync(pending.Id)).ReturnsAsync(pending);
        repo.Setup(r => r.UpdateAsync(pending)).ReturnsAsync(pending);
        // Act
        var acc = await s.AcceptFriendRequestAsync(pending.Id, "u2");
        // Assert
        acc.Status.Should().Be("Accepted");

        pending.Status = FriendshipStatus.Pending;
        repo.Setup(r => r.GetByIdAsync(pending.Id)).ReturnsAsync(pending);
        repo.Setup(r => r.UpdateAsync(pending)).ReturnsAsync(pending);
        // Act
        var rej = await s.RejectFriendRequestAsync(pending.Id, "u2");
        // Assert
        rej.Status.Should().Be("Rejected");
    }

    [Fact]
    public async Task SendFriendRequestAsync_Should_Throw_On_Self_Existing_Or_MissingReceiver()
    {
        // Arrange
        var s = CreateService(out var repo, out var users, out var current);
        // Act & Assert
        await FluentActions.Awaiting(() => s.SendFriendRequestAsync("u1", "u1")).Should().ThrowAsync<ArgumentException>();

        repo.Setup(r => r.GetFriendshipAsync("u1", "u2")).ReturnsAsync(new Friendship());
        await FluentActions.Awaiting(() => s.SendFriendRequestAsync("u1", "u2")).Should().ThrowAsync<InvalidOperationException>();

        repo.Reset();
        repo.Setup(r => r.GetFriendshipAsync("u1", "u3")).ReturnsAsync((Friendship?)null);
        users.Setup(u => u.GetByIdAsync("u3")).ReturnsAsync((User?)null);
        await FluentActions.Awaiting(() => s.SendFriendRequestAsync("u1", "u3")).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AcceptRejectRemove_Should_Throw_On_InvalidState()
    {
        // Arrange
        var s = CreateService(out var repo, out var users, out var current);
        var id = Guid.NewGuid();
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Friendship?)null);
        await FluentActions.Awaiting(() => s.AcceptFriendRequestAsync(id, "u2")).Should().ThrowAsync<ArgumentException>();

        var other = new Friendship { Id = id, ReceiverId = "x", Status = FriendshipStatus.Pending };
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(other);
        await FluentActions.Awaiting(() => s.AcceptFriendRequestAsync(id, "u2")).Should().ThrowAsync<UnauthorizedAccessException>();

        var nonPending = new Friendship { Id = id, ReceiverId = "u2", Status = FriendshipStatus.Accepted };
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(nonPending);
        await FluentActions.Awaiting(() => s.AcceptFriendRequestAsync(id, "u2")).Should().ThrowAsync<InvalidOperationException>();

        var fr = new Friendship { Id = Guid.NewGuid(), Status = FriendshipStatus.Pending };
        repo.Setup(r => r.GetByIdAsync(fr.Id)).ReturnsAsync((Friendship?)null);
        await FluentActions.Awaiting(() => s.RejectFriendRequestAsync(fr.Id, "u2")).Should().ThrowAsync<ArgumentException>();

        var wrongUser = new Friendship { Id = fr.Id, ReceiverId = "x", Status = FriendshipStatus.Pending };
        repo.Setup(r => r.GetByIdAsync(fr.Id)).ReturnsAsync(wrongUser);
        await FluentActions.Awaiting(() => s.RejectFriendRequestAsync(fr.Id, "u2")).Should().ThrowAsync<UnauthorizedAccessException>();

        var notPending = new Friendship { Id = fr.Id, ReceiverId = "u2", Status = FriendshipStatus.Accepted };
        repo.Setup(r => r.GetByIdAsync(fr.Id)).ReturnsAsync(notPending);
        await FluentActions.Awaiting(() => s.RejectFriendRequestAsync(fr.Id, "u2")).Should().ThrowAsync<InvalidOperationException>();

        repo.Setup(r => r.GetFriendshipAsync("a", "b")).ReturnsAsync((Friendship?)null);
        await FluentActions.Awaiting(() => s.RemoveFriendAsync("a", "b")).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RemoveFriendAsync_And_AreFriendsAsync_Should_Work()
    {
        // Arrange
        var s = CreateService(out var repo, out var users, out var current);
        var fr = new Friendship { Id = Guid.NewGuid(), SenderId = "a", ReceiverId = "b", Status = FriendshipStatus.Accepted };
        repo.Setup(r => r.GetFriendshipAsync("a", "b")).ReturnsAsync(fr);
        repo.Setup(r => r.DeleteAsync(fr.Id)).Returns(Task.CompletedTask);
        // Act
        await s.RemoveFriendAsync("a", "b");

        repo.Setup(r => r.AreFriendsAsync("a", "b")).ReturnsAsync(true);
        // Assert
        (await s.AreFriendsAsync("a", "b")).Should().BeTrue();
    }

    [Fact]
    public async Task Http_Wrappers_Should_Cover_Branches()
    {
        // Arrange
        var s = CreateService(out var repo, out var users, out var current);
        var http = new DefaultHttpContext();

        // Act & Assert - GetFriends unauthorized
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await s.GetFriendsAsync(http)).Should().BeOfType<UnauthorizedObjectResult>();

        // Act & Assert - Authorized paths
        current.Setup(c => c.GetCurrentUserId()).Returns("me");
        repo.Setup(r => r.GetUserFriendsAsync("me")).ReturnsAsync(new List<Friendship>());
        (await s.GetFriendsAsync(http)).Should().BeOfType<OkObjectResult>();

        repo.Setup(r => r.GetPendingRequestsReceivedAsync("me")).ReturnsAsync(new List<Friendship>());
        users.Setup(u => u.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new User { Id = "x", Name = "X" });
        (await s.GetPendingRequestsAsync(http)).Should().BeOfType<OkObjectResult>();

        users.Setup(u => u.GetAllAsync(1, 1000)).ReturnsAsync(new List<User> { new User { Id = "me" }, new User { Id = "x" } });
        repo.Setup(r => r.GetFriendUserIdsAsync("me")).ReturnsAsync(new List<string>());
        repo.Setup(r => r.GetPendingRequestsSentAsync("me")).ReturnsAsync(new List<Friendship>());
        repo.Setup(r => r.GetPendingRequestsReceivedAsync("me")).ReturnsAsync(new List<Friendship>());
        (await s.GetAvailableUsersAsync(http)).Should().BeOfType<OkObjectResult>();

        // Act & Assert - Send request unauthorized
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await s.SendFriendRequestAsync(http, new CreateFriendRequestDto { ReceiverId = "x" })).Should().BeOfType<UnauthorizedObjectResult>();

        // Act & Assert - Accept/Reject unauthorized
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await s.AcceptFriendRequestAsync(http, Guid.NewGuid())).Should().BeOfType<UnauthorizedObjectResult>();
        (await s.RejectFriendRequestAsync(http, Guid.NewGuid())).Should().BeOfType<UnauthorizedObjectResult>();

        // Act & Assert - Remove unauthorized
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await s.RemoveFriendAsync(http, "x")).Should().BeOfType<UnauthorizedObjectResult>();

        // Act & Assert - Check friendship unauthorized
        current.Setup(c => c.GetCurrentUserId()).Returns((string?)null);
        (await s.CheckFriendshipAsync(http, "x")).Should().BeOfType<UnauthorizedObjectResult>();
    }
}


