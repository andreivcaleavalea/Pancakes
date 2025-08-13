using UserService.Helpers;
using UserService.Models.Entities;
using UserService.Models.DTOs;

namespace UserService.Tests.Mapping;

public class MappingProfileTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return new Mapper(cfg);
    }

    [Fact]
    public void User_To_UserDto_Maps_Ban_Status_And_Metrics()
    {
        var mapper = CreateMapper();
        var user = new User
        {
            Id = "u1",
            Name = "Alice",
            Email = "a@x.com",
            Bans = new List<Ban>
            {
                new Ban { IsActive = true, Reason = "spam", CreatedAt = new DateTime(2025,1,2), BannedAt = new DateTime(2025,1,2) },
                new Ban { IsActive = false, Reason = "old" }
            }
        };

        var dto = mapper.Map<UserDto>(user);
        dto.IsBanned.Should().BeTrue();
        dto.CurrentBanReason.Should().Be("spam");
        dto.BanHistoryCount.Should().Be(2);
    }
}


