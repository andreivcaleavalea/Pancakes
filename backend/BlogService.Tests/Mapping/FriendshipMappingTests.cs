using System;
using AutoMapper;
using BlogService.Helpers;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;

namespace BlogService.Tests.Mapping;

public class FriendshipMappingTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return new Mapper(config);
    }

    [Fact]
    public void Friendship_Maps_To_Dto_With_Status_String_And_Timestamps()
    {
        var mapper = CreateMapper();

        var entity = new Friendship
        {
            Id = Guid.NewGuid(),
            SenderId = "user-a",
            ReceiverId = "user-b",
            Status = FriendshipStatus.Accepted,
            CreatedAt = new DateTime(2025, 8, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 8, 2, 12, 0, 0, DateTimeKind.Utc),
            AcceptedAt = new DateTime(2025, 8, 3, 12, 0, 0, DateTimeKind.Utc)
        };

        var dto = mapper.Map<FriendshipDto>(entity);

        dto.Id.Should().Be(entity.Id);
        dto.SenderId.Should().Be("user-a");
        dto.ReceiverId.Should().Be("user-b");
        dto.Status.Should().Be("Accepted");
        dto.CreatedAt.Should().Be(entity.CreatedAt);
        dto.AcceptedAt.Should().Be(entity.AcceptedAt);
        dto.SenderInfo.Should().BeNull();
        dto.ReceiverInfo.Should().BeNull();
    }

    [Fact]
    public void Friendship_Pending_Maps_To_Dto_With_Null_AcceptedAt()
    {
        var mapper = CreateMapper();

        var entity = new Friendship
        {
            SenderId = "u1",
            ReceiverId = "u2",
            Status = FriendshipStatus.Pending,
            AcceptedAt = null
        };

        var dto = mapper.Map<FriendshipDto>(entity);

        dto.Status.Should().Be("Pending");
        dto.AcceptedAt.Should().BeNull();
    }
}


