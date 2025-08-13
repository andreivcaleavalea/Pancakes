using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UserService.Services.Implementations;

namespace UserService.Tests.Services;

public class OAuthServiceAdditionalTests
{
	[Fact]
	public async Task ExchangeCodeForUserInfo_Returns_Null_On_NonSuccess_Token()
	{
		var handler = new DelegatingHandlerStub(async (req, ct) =>
		{
			await Task.Yield();
			return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
			{
				Content = new StringContent("error=bad")
			};
		});
		var http = new HttpClient(handler);
		var config = new ConfigurationBuilder().Build();
		Environment.SetEnvironmentVariable("OAUTH_GITHUB_CLIENT_ID", "x");
		Environment.SetEnvironmentVariable("OAUTH_GITHUB_CLIENT_SECRET", "y");
		var svc = new OAuthService(http, config);
		var res = await svc.ExchangeCodeForUserInfo("code", "github");
		res.Should().BeNull();
	}

	[Fact]
	public async Task GetUserInfo_GitHub_Email_Array_Error_Does_Not_Throw()
	{
		var step = 0;
		var handler = new DelegatingHandlerStub(async (req, ct) =>
		{
			await Task.Yield();
			step++;
			if (step == 1)
			{
				// First call to /user
				return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
				{
					Content = new StringContent("{\"id\":1,\"login\":\"gh\"}")
				};
			}
			// Second call to /user/emails returns object instead of array
			return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
			{
				Content = new StringContent("{\"email\":\"x@x.com\"}")
			};
		});
		var http = new HttpClient(handler);
		var config = new ConfigurationBuilder().Build();
		Environment.SetEnvironmentVariable("OAUTH_GITHUB_CLIENT_ID", "x");
		Environment.SetEnvironmentVariable("OAUTH_GITHUB_CLIENT_SECRET", "y");
		var svc = new OAuthService(http, config);
		var info = await svc.GetUserInfo("abc", "github");
		info.Should().NotBeNull();
		info!.Id.Should().Be("1");
	}

	private sealed class DelegatingHandlerStub : DelegatingHandler
	{
		private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;
		public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
		{
			_responder = responder;
		}
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return _responder(request, cancellationToken);
		}
	}
}


