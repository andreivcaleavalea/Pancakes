using UserService.Services.Implementations;
using UserService.Models.DTOs;

namespace UserService.Tests.Services;

public class UserMappingServiceTests
{
    [Fact]
    public void MapUserDtoToUser_Maps_All_Fields()
    {
        var svc = new UserMappingService();
        var dto = new UserDto
        {
            Id = "u1",
            Name = "A",
            Email = "a@x.com",
            Image = "i",
            Provider = "p",
            ProviderUserId = "pid",
            Bio = "bio",
            PhoneNumber = "+1",
            DateOfBirth = new DateTime(2000,1,2),
            CreatedAt = new DateTime(2024,1,1),
            LastLoginAt = new DateTime(2024,2,1),
            UpdatedAt = new DateTime(2024,3,1)
        };
        var user = svc.MapUserDtoToUser(dto);
        user.Id.Should().Be("u1");
        user.Name.Should().Be("A");
        user.Email.Should().Be("a@x.com");
        user.Provider.Should().Be("p");
        user.ProviderUserId.Should().Be("pid");
        user.CreatedAt.Should().Be(dto.CreatedAt);
        user.LastLoginAt.Should().Be(dto.LastLoginAt);
        user.UpdatedAt.Should().Be(dto.UpdatedAt);
    }
}


