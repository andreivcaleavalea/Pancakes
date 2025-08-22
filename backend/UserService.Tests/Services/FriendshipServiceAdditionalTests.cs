using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Implementations;
using UserService.Services.Interfaces;

namespace UserService.Tests.Services;

public class FriendshipServiceAdditionalTests
{
    private (FriendshipService svc, Mock<IFriendshipRepository> fRepo, Mock<IUserRepository> uRepo, Mock<ICurrentUserService> current, Mock<INotificationService> notif) Build(
        IEnumerable<User>? users = null,
        IEnumerable<Friendship>? friendships = null,
        IEnumerable<Friendship>? pendingRecv = null,
        IEnumerable<Friendship>? pendingSent = null)
    {
        var fRepo = new Mock<IFriendshipRepository>();
        var uRepo = new Mock<IUserRepository>();
        var current = new Mock<ICurrentUserService>();
        var notif = new Mock<INotificationService>();
        var mapper = new MapperConfiguration(cfg => { cfg.CreateMap<Friendship, FriendshipDto>(); }).CreateMapper();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FriendshipService>>().Object;
        users ??= Array.Empty<User>();
        friendships ??= Array.Empty<Friendship>();
        pendingRecv ??= Array.Empty<Friendship>();
        pendingSent ??= Array.Empty<Friendship>();
        foreach (var u in users)
            uRepo.Setup(r => r.GetByIdAsync(u.Id)).ReturnsAsync(u);
        fRepo.Setup(r => r.GetUserFriendsAsync(It.IsAny<string>())).ReturnsAsync(friendships);
        fRepo.Setup(r => r.GetPendingRequestsReceivedAsync(It.IsAny<string>())).ReturnsAsync(pendingRecv);
        fRepo.Setup(r => r.GetPendingRequestsSentAsync(It.IsAny<string>())).ReturnsAsync(pendingSent);
        fRepo.Setup(r => r.GetFriendUserIdsAsync(It.IsAny<string>())).ReturnsAsync(friendships.Select(f => f.SenderId).Concat(friendships.Select(f => f.ReceiverId)).Distinct().ToList());
        var svc = new FriendshipService(fRepo.Object, uRepo.Object, mapper, current.Object, notif.Object, logger);
        return (svc, fRepo, uRepo, current, notif);
    }

    [Fact]
    public async Task GetAvailableUsersAsync_Filters_Friends_And_Pending()
    {
        var userA = new User { Id = "A", Name = "A" };
        var userB = new User { Id = "B", Name = "B" };
        var userC = new User { Id = "C", Name = "C" }; // pending
        var userD = new User { Id = "D", Name = "D" }; // should remain
        var friendship = new Friendship { Id = Guid.NewGuid(), SenderId = "A", ReceiverId = "B", Status = FriendshipStatus.Accepted, CreatedAt = DateTime.UtcNow };
        var pending = new Friendship { Id = Guid.NewGuid(), SenderId = "A", ReceiverId = "C", Status = FriendshipStatus.Pending, CreatedAt = DateTime.UtcNow };
        var (svc, fRepo, uRepo, current, _) = Build(
            users: new[] { userA, userB, userC, userD },
            friendships: new[] { friendship },
            pendingSent: new[] { pending }
        );
        uRepo.Setup(r => r.GetAllAsync(1, 1000)).ReturnsAsync(new[] { userA, userB, userC, userD });
        current.Setup(c => c.GetCurrentUserId()).Returns("A");
        var res = await svc.GetAvailableUsersAsync(new DefaultHttpContext()) as OkObjectResult;
        res.Should().NotBeNull();
        var list = res!.Value as IEnumerable<AvailableUserDto>;
        list!.Select(u => u.Id).Should().BeEquivalentTo(new[] { "D" });
    }

    [Fact]
    public async Task SendFriendRequestAsync_ArgumentErrors_And_Conflict()
    {
        var (svc, fRepo, uRepo, current, notif) = Build();
        current.Setup(c => c.GetCurrentUserId()).Returns("A");
        // self request
        var bad = await svc.SendFriendRequestAsync(new DefaultHttpContext(), new CreateFriendRequestDto { ReceiverId = "A" });
        bad.Should().BeOfType<BadRequestObjectResult>();
        // receiver missing
        fRepo.Setup(r => r.GetFriendshipAsync("A", "B")).ReturnsAsync((Friendship?)null);
        uRepo.Setup(r => r.GetByIdAsync("B")).ReturnsAsync((User?)null);
        var missing = await svc.SendFriendRequestAsync(new DefaultHttpContext(), new CreateFriendRequestDto { ReceiverId = "B" });
        missing.Should().BeOfType<BadRequestObjectResult>();
        // existing friendship
        uRepo.Setup(r => r.GetByIdAsync("C")).ReturnsAsync(new User { Id = "C", Name = "C" });
        fRepo.Setup(r => r.GetFriendshipAsync("A", "C")).ReturnsAsync(new Friendship { Id = Guid.NewGuid(), SenderId = "A", ReceiverId = "C", Status = FriendshipStatus.Accepted });
        var conflict = await svc.SendFriendRequestAsync(new DefaultHttpContext(), new CreateFriendRequestDto { ReceiverId = "C" });
        conflict.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task AcceptFriendRequestAsync_Errors()
    {
        var (svc, fRepo, _, current, _) = Build();
        current.Setup(c => c.GetCurrentUserId()).Returns("B");
        var id = Guid.NewGuid();
        // Not found
        var nf = await svc.AcceptFriendRequestAsync(new DefaultHttpContext(), id);
        nf.Should().BeOfType<NotFoundObjectResult>();
        // Wrong receiver (unauthorized)
        fRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Friendship { Id = id, SenderId = "A", ReceiverId = "C", Status = FriendshipStatus.Pending });
        var forbid = await svc.AcceptFriendRequestAsync(new DefaultHttpContext(), id);
        forbid.Should().BeOfType<ForbidResult>();
        // Invalid state
        fRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Friendship { Id = id, SenderId = "A", ReceiverId = "B", Status = FriendshipStatus.Accepted });
        var bad = await svc.AcceptFriendRequestAsync(new DefaultHttpContext(), id);
        bad.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RejectFriendRequestAsync_Errors()
    {
        var (svc, fRepo, _, current, _) = Build();
        current.Setup(c => c.GetCurrentUserId()).Returns("B");
        var id = Guid.NewGuid();
        var nf = await svc.RejectFriendRequestAsync(new DefaultHttpContext(), id);
        nf.Should().BeOfType<NotFoundObjectResult>();
        fRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Friendship { Id = id, SenderId = "A", ReceiverId = "C", Status = FriendshipStatus.Pending });
        var forbid = await svc.RejectFriendRequestAsync(new DefaultHttpContext(), id);
        forbid.Should().BeOfType<ForbidResult>();
        fRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Friendship { Id = id, SenderId = "A", ReceiverId = "B", Status = FriendshipStatus.Accepted });
        var bad = await svc.RejectFriendRequestAsync(new DefaultHttpContext(), id);
        bad.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RemoveFriendAsync_NotFound_And_Invalid()
    {
        var (svc, fRepo, _, current, _) = Build();
        current.Setup(c => c.GetCurrentUserId()).Returns("A");
        var nf = await svc.RemoveFriendAsync(new DefaultHttpContext(), "B");
        nf.Should().BeOfType<NotFoundObjectResult>();
        fRepo.Setup(r => r.GetFriendshipAsync("A", "B")).ReturnsAsync(new Friendship { Id = Guid.NewGuid(), SenderId = "A", ReceiverId = "B", Status = FriendshipStatus.Pending });
        var bad = await svc.RemoveFriendAsync(new DefaultHttpContext(), "B");
        bad.Should().BeOfType<NotFoundObjectResult>(); // underlying ArgumentException mapped to 404
    }

    [Fact]
    public async Task CheckFriendshipAsync_Ok_Path()
    {
        var (svc, fRepo, _, current, _) = Build();
        current.Setup(c => c.GetCurrentUserId()).Returns("A");
        fRepo.Setup(r => r.AreFriendsAsync("A", "B")).ReturnsAsync(true);
        var ok = await svc.CheckFriendshipAsync(new DefaultHttpContext(), "B") as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be(true);
    }

    // ---------------- Additional success path coverage below ----------------

    [Fact]
    public async Task SendFriendRequestAsync_Success_Path_And_Notification_Exception()
    {
        var friendshipRepo = new Mock<IFriendshipRepository>();
        var userRepo = new Mock<IUserRepository>();
        var current = new Mock<ICurrentUserService>();
        var notif = new Mock<INotificationService>();
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Friendship, FriendshipDto>()).CreateMapper();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FriendshipService>>().Object;
        current.Setup(c => c.GetCurrentUserId()).Returns("A");
        friendshipRepo.Setup(r => r.GetFriendshipAsync("A", "B")).ReturnsAsync((Friendship?)null);
        userRepo.Setup(r => r.GetByIdAsync("B")).ReturnsAsync(new User { Id = "B", Name = "Bee" });
        userRepo.Setup(r => r.GetByIdAsync("A")).ReturnsAsync(new User { Id = "A", Name = "Ay" });
        friendshipRepo.Setup(r => r.CreateAsync(It.IsAny<Friendship>())).ReturnsAsync((Friendship f) => { f.Id = Guid.NewGuid(); return f; });
        // Force notification creation to throw to cover catch path
        notif.Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>())).ThrowsAsync(new Exception("notify fail"));
        var svc = new FriendshipService(friendshipRepo.Object, userRepo.Object, mapper, current.Object, notif.Object, logger);
        var res = await svc.SendFriendRequestAsync(new DefaultHttpContext(), new CreateFriendRequestDto { ReceiverId = "B" });
        res.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AcceptFriendRequestAsync_Success_Path_With_Notification()
    {
        var friendshipRepo = new Mock<IFriendshipRepository>();
        var userRepo = new Mock<IUserRepository>();
        var current = new Mock<ICurrentUserService>();
        var notif = new Mock<INotificationService>();
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Friendship, FriendshipDto>()).CreateMapper();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FriendshipService>>().Object;
        current.Setup(c => c.GetCurrentUserId()).Returns("B");
        var id = Guid.NewGuid();
        var pending = new Friendship { Id = id, SenderId = "A", ReceiverId = "B", Status = FriendshipStatus.Pending, CreatedAt = DateTime.UtcNow };
        friendshipRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pending);
        friendshipRepo.Setup(r => r.UpdateAsync(It.IsAny<Friendship>())).ReturnsAsync((Friendship f) => f);
        userRepo.Setup(r => r.GetByIdAsync("B")).ReturnsAsync(new User { Id = "B", Name = "Bee" });
        notif.Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>())).ReturnsAsync(new NotificationDto());
        var svc = new FriendshipService(friendshipRepo.Object, userRepo.Object, mapper, current.Object, notif.Object, logger);
        var res = await svc.AcceptFriendRequestAsync(new DefaultHttpContext(), id);
        res.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RejectFriendRequestAsync_Success_Path()
    {
        var friendshipRepo = new Mock<IFriendshipRepository>();
        var userRepo = new Mock<IUserRepository>();
        var current = new Mock<ICurrentUserService>();
        var notif = new Mock<INotificationService>();
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Friendship, FriendshipDto>()).CreateMapper();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FriendshipService>>().Object;
        current.Setup(c => c.GetCurrentUserId()).Returns("B");
        var id = Guid.NewGuid();
        var pending = new Friendship { Id = id, SenderId = "A", ReceiverId = "B", Status = FriendshipStatus.Pending, CreatedAt = DateTime.UtcNow };
        friendshipRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pending);
        friendshipRepo.Setup(r => r.UpdateAsync(It.IsAny<Friendship>())).ReturnsAsync((Friendship f) => f);
        var svc = new FriendshipService(friendshipRepo.Object, userRepo.Object, mapper, current.Object, notif.Object, logger);
        var res = await svc.RejectFriendRequestAsync(new DefaultHttpContext(), id);
        res.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoveFriendAsync_Success_Path()
    {
        var friendshipRepo = new Mock<IFriendshipRepository>();
        var userRepo = new Mock<IUserRepository>();
        var current = new Mock<ICurrentUserService>();
        var notif = new Mock<INotificationService>();
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Friendship, FriendshipDto>()).CreateMapper();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FriendshipService>>().Object;
        current.Setup(c => c.GetCurrentUserId()).Returns("A");
        friendshipRepo.Setup(r => r.GetFriendshipAsync("A", "B")).ReturnsAsync(new Friendship { Id = Guid.NewGuid(), SenderId = "A", ReceiverId = "B", Status = FriendshipStatus.Accepted, CreatedAt = DateTime.UtcNow });
        friendshipRepo.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        var svc = new FriendshipService(friendshipRepo.Object, userRepo.Object, mapper, current.Object, notif.Object, logger);
        var res = await svc.RemoveFriendAsync(new DefaultHttpContext(), "B");
        res.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetFriendsAndPendingRequests_Success_Paths()
    {
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.GetCurrentUserId()).Returns("A");
        var friendshipRepo = new Mock<IFriendshipRepository>();
        var userRepo = new Mock<IUserRepository>();
        var notif = new Mock<INotificationService>();
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Friendship, FriendshipDto>()).CreateMapper();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FriendshipService>>().Object;
        var f1 = new Friendship { Id = Guid.NewGuid(), SenderId = "A", ReceiverId = "B", Status = FriendshipStatus.Accepted, CreatedAt = DateTime.UtcNow };
        friendshipRepo.Setup(r => r.GetUserFriendsAsync("A")).ReturnsAsync(new[] { f1 });
        userRepo.Setup(r => r.GetByIdAsync("B")).ReturnsAsync(new User { Id = "B", Name = "Bee", Image = "img" });
        var pending = new Friendship { Id = Guid.NewGuid(), SenderId = "C", ReceiverId = "A", Status = FriendshipStatus.Pending, CreatedAt = DateTime.UtcNow };
        friendshipRepo.Setup(r => r.GetPendingRequestsReceivedAsync("A")).ReturnsAsync(new[] { pending });
        userRepo.Setup(r => r.GetByIdAsync("C")).ReturnsAsync(new User { Id = "C", Name = "See", Image = "img2" });
        var svc = new FriendshipService(friendshipRepo.Object, userRepo.Object, mapper, current.Object, notif.Object, logger);
        var friendsRes = await svc.GetFriendsAsync(new DefaultHttpContext()) as OkObjectResult;
        var pendingRes = await svc.GetPendingRequestsAsync(new DefaultHttpContext()) as OkObjectResult;
        friendsRes.Should().NotBeNull();
        pendingRes.Should().NotBeNull();
    }
}
