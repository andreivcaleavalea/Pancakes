using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BlogService.Models.DTOs;
using BlogService.Services.Implementations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class UserServiceClientTests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public int CallCount { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastContent { get; private set; }

        public CapturingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            if (request.Content != null)
            {
                LastContent = await request.Content.ReadAsStringAsync(cancellationToken);
            }
            return await _handler(request, cancellationToken);
        }
    }

    private static (UserServiceClient client, CapturingHandler handler, HttpClient httpClient, IMemoryCache cache) CreateClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
    {
        var handler = new CapturingHandler(responder);
        var httpClient = new HttpClient(handler);
        var config = new ConfigurationBuilder().Build();
        var logger = new Mock<ILogger<UserServiceClient>>().Object;
        var cache = new MemoryCache(new MemoryCacheOptions());

        var client = new UserServiceClient(httpClient, config, (ILogger<UserServiceClient>)logger, cache);
        return (client, handler, httpClient, cache);
    }

    [Fact]
    public async Task GetCurrentUserAsync_Sets_Authorization_And_Parses_User()
    {
        var (client, handler, httpClient, _) = CreateClient(async (req, ct) =>
        {
            await Task.Yield();
            req.Headers.Authorization.Should().NotBeNull();
            req.RequestUri!.AbsolutePath.Should().Be("/auth/me");
            var payload = JsonSerializer.Serialize(new { id = "u1", name = "Alice", email = "a@x.com", image = "img" });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        });

        var user = await client.GetCurrentUserAsync("tok");

        user.Should().NotBeNull();
        user!.Id.Should().Be("u1");
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetUserById_Caches_Result_After_First_Call()
    {
        int responses = 0;
        var (client, handler, _, _) = CreateClient(async (req, ct) =>
        {
            await Task.Yield();
            responses++;
            var payload = JsonSerializer.Serialize(new { id = "u42", name = "Bob" });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        });

        var first = await client.GetUserByIdAsync("u42");
        var second = await client.GetUserByIdAsync("u42");

        first!.Id.Should().Be("u42");
        second!.Id.Should().Be("u42");
        handler.CallCount.Should().Be(1);
        responses.Should().Be(1);
    }

    [Fact]
    public async Task GetUserById_WithAuth_Sets_And_Clears_Header()
    {
        var (client, handler, httpClient, _) = CreateClient(async (req, ct) =>
        {
            await Task.Yield();
            req.Headers.Authorization.Should().NotBeNull();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { id = "u7" }), Encoding.UTF8, "application/json")
            };
        });

        var user = await client.GetUserByIdAsync("u7", "tok");
        user!.Id.Should().Be("u7");

        // Authorization header should be cleared by the client after call
        httpClient.DefaultRequestHeaders.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersByIds_PartialCache_Posts_Only_Uncached_And_Caches_New()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var (client, handler, _, cache) = CreateClient(async (req, ct) =>
        {
            await Task.Yield();
            req.Method.Should().Be(HttpMethod.Post);
            req.RequestUri!.AbsolutePath.Should().Be("/users/batch");

            // Respond with users for the posted, uncached IDs
            var postedIds = JsonSerializer.Deserialize<List<string>>(await req.Content!.ReadAsStringAsync(ct));
            var users = postedIds!.Select(id => new { id, name = $"Name-{id}" }).ToList();
            var payload = JsonSerializer.Serialize(users, options);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        });

        // Pre-cache one user
        cache.Set("user_by_id_u1", new UserInfoDto { Id = "u1", Name = "Cached" });

        var result = (await client.GetUsersByIdsAsync(new[] { "u1", "u2", "u3" })).ToList();
        result.Select(u => u.Id).Should().BeEquivalentTo(new[] { "u1", "u2", "u3" });
        handler.CallCount.Should().Be(1);

        // All cached now: second call should not hit HTTP
        var again = (await client.GetUsersByIdsAsync(new[] { "u1", "u2", "u3" })).ToList();
        again.Select(u => u.Id).Should().BeEquivalentTo(new[] { "u1", "u2", "u3" });
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetUserFriendsAsync_Returns_Friends_And_Clears_Header()
    {
        var (client, handler, httpClient, _) = CreateClient(async (req, ct) =>
        {
            await Task.Yield();
            req.Headers.Authorization.Should().NotBeNull();
            req.RequestUri!.AbsolutePath.Should().Be("/api/friendships/friends");

            var friends = new object[]
            {
                new { userId = "u1", name = "A", image = "i1.png", friendsSince = new DateTime(2024,1,1) },
                new { userId = "u2", name = "B", image = (string?)null, friendsSince = new DateTime(2024,1,2) }
            };
            var payload = JsonSerializer.Serialize(friends, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        });

        var friendsResult = (await client.GetUserFriendsAsync("tok")).ToList();
        friendsResult.Should().HaveCount(2);
        friendsResult[0].UserId.Should().Be("u1");
        friendsResult[0].Name.Should().Be("A");
        friendsResult[0].Image.Should().Be("i1.png");
        friendsResult[1].UserId.Should().Be("u2");
        friendsResult[1].Image.Should().BeNull();

        httpClient.DefaultRequestHeaders.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task AreFriendsAsync_Returns_True_On_Success_And_False_Otherwise()
    {
        // First: success true
        var (clientTrue, handlerTrue, httpTrue, _) = CreateClient(async (req, ct) =>
        {
            await Task.Yield();
            req.RequestUri!.AbsolutePath.Should().StartWith("/api/friendships/check-friendship/");
            var payload = JsonSerializer.Serialize(new { areFriends = true });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        });

        var tf1 = await clientTrue.AreFriendsAsync("a", "b", "tok");
        tf1.Should().BeTrue();
        httpTrue.DefaultRequestHeaders.Authorization.Should().BeNull();

        // Second: non-success -> false
        var (clientFalse, handlerFalse, httpFalse, _) = CreateClient(async (req, ct) =>
        {
            await Task.Yield();
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var tf2 = await clientFalse.AreFriendsAsync("a", "b", "tok");
        tf2.Should().BeFalse();
        httpFalse.DefaultRequestHeaders.Authorization.Should().BeNull();
    }
}


