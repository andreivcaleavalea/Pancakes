using UserService.Models.DTOs;

namespace UserService.Tests.Models;

public class DtoShapeSmokeTests
{
	[Fact]
	public void AvailableUserDto_Defaults_Are_Set()
	{
		var dto = new AvailableUserDto();
		dto.Id.Should().BeEmpty();
		dto.Name.Should().BeEmpty();
		dto.Email.Should().BeEmpty();
		dto.Image.Should().BeNull();
		dto.Bio.Should().BeNull();
	}

	[Fact]
	public void FriendRequestDto_Simple_Setters()
	{
		var id = Guid.NewGuid();
		var dto = new FriendRequestDto
		{
			Id = id,
			SenderId = "u1",
			SenderName = "Alice",
			SenderImage = "a.png",
			CreatedAt = DateTime.UtcNow
		};
		dto.Id.Should().Be(id);
		dto.SenderId.Should().Be("u1");
		dto.SenderName.Should().Be("Alice");
		dto.SenderImage.Should().Be("a.png");
	}
}


