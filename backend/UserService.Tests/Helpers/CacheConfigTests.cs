using UserService.Configuration;

namespace UserService.Tests.Helpers;

public class CacheConfigTests
{
	[Fact]
	public void FormatKey_And_CreateHash_Work()
	{
		var key = CacheConfig.FormatKey(CacheConfig.Keys.UserById, "u1");
		key.Should().Be("user_by_id_{0}".Replace("{0}", "u1"));

		var h1 = CacheConfig.CreateHash(new[] { "b", "a" });
		var h2 = CacheConfig.CreateHash(new[] { "a", "b" });
		h1.Should().Be(h2);
	}
}


