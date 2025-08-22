using Microsoft.Extensions.Configuration;
using UserService.Services.Implementations;
using UserService.Services.Interfaces;
using UserService.Models.Authentication;

namespace UserService.Tests.Services;

public class OAuthServiceEmailTests
{
    private OAuthService Build(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new DelegatingHandlerStub((req, ct) => Task.FromResult(responder(req)));
        var http = new HttpClient(handler);
        var config = new ConfigurationBuilder().Build();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<OAuthService>>().Object;
        return new OAuthService(http, config, logger);
    }

    [Fact]
    public async Task GetGitHubUserEmail_PrimaryVerified_ReturnsPrimary()
    {
        var emailsJson = "[{\"email\":\"p@x.com\",\"primary\":true,\"verified\":true},{\"email\":\"s@x.com\",\"primary\":false,\"verified\":true}]";
        var userJson = "{\"id\":1,\"login\":\"u\",\"email\":null}"; // force email fetch
        var seq = new Queue<HttpResponseMessage>(new []{
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(userJson)},
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(emailsJson)}
        });
        var svc = Build((req)=> seq.Dequeue());
        var info = await svc.GetUserInfo("token","github");
        info!.Email.Should().Be("p@x.com");
    }

    [Fact]
    public async Task GetGitHubUserEmail_NoPrimary_UsesFirstVerified()
    {
        var emailsJson = "[{\"email\":\"a@x.com\",\"primary\":false,\"verified\":true},{\"email\":\"b@x.com\",\"primary\":false,\"verified\":false}]";
        var userJson = "{\"id\":2,\"login\":\"u2\",\"email\":null}";
        var seq = new Queue<HttpResponseMessage>(new []{
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(userJson)},
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(emailsJson)}
        });
        var svc = Build((req)=> seq.Dequeue());
        var info = await svc.GetUserInfo("token","github");
        info!.Email.Should().Be("a@x.com");
    }

    [Fact]
    public async Task GetGitHubUserEmail_NoVerified_ReturnsEmpty()
    {
        var emailsJson = "[{\"email\":\"a@x.com\",\"primary\":false,\"verified\":false}]";
        var userJson = "{\"id\":3,\"login\":\"u3\",\"email\":null}";
        var seq = new Queue<HttpResponseMessage>(new []{
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(userJson)},
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(emailsJson)}
        });
        var svc = Build((req)=> seq.Dequeue());
        var info = await svc.GetUserInfo("token","github");
        info!.Email.Should().Be("");
    }

    [Fact]
    public async Task GetGitHubUserEmail_EmailsEndpoint_Failure_ReturnsEmpty()
    {
        var userJson = "{\"id\":4,\"login\":\"u4\",\"email\":null}";
        var seq = new Queue<HttpResponseMessage>(new []{
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(userJson)},
            new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError){Content=new StringContent("oops")}
        });
        var svc = Build((req)=> seq.Dequeue());
        var info = await svc.GetUserInfo("token","github");
        info!.Email.Should().Be("");
    }

    [Fact]
    public async Task GetGitHubUserEmail_MalformedJson_Caught_ReturnsEmpty()
    {
        var userJson = "{\"id\":5,\"login\":\"u5\",\"email\":null}";
        var badEmails = "not-json";
        var seq = new Queue<HttpResponseMessage>(new []{
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(userJson)},
            new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(badEmails)}
        });
        var svc = Build((req)=> seq.Dequeue());
        var info = await svc.GetUserInfo("token","github");
        info!.Email.Should().Be("");
    }

    private sealed class DelegatingHandlerStub : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;
        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _responder(request, cancellationToken);
    }
}
