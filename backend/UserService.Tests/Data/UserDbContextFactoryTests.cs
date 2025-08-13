using UserService.Data;

namespace UserService.Tests.Data;

public class UserDbContextFactoryTests
{
	[Fact]
	public void Factory_Creates_Context()
	{
		var factory = new UserDbContextFactory();
		var ctx = factory.CreateDbContext(Array.Empty<string>());
		ctx.Should().NotBeNull();
		ctx.Dispose();
	}
}


