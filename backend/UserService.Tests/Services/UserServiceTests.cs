using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using UserService.Services.Implementations;
using UserService.Services.Interfaces;
using UserService.Repositories.Interfaces;
using UserService.Models.Entities;
using UserService.Models.DTOs;
using UserService.Helpers;

namespace UserService.Tests.Services;

public class UserServiceTests
{
    private static UserService.Services.Implementations.UserService CreateService(
        out Mock<IUserRepository> repo,
        out Mock<IBanService> banService)
    {
        repo = new Mock<IUserRepository>(MockBehavior.Strict);
        banService = new Mock<IBanService>(MockBehavior.Strict);
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<UserService.Services.Implementations.UserService>>().Object;
        var picFactory = new Mock<IProfilePictureStrategyFactory>(MockBehavior.Loose);
        picFactory.Setup(f => f.GetStrategy(It.IsAny<string>())).Returns(new Mock<IProfilePictureStrategy>().Object);
        return new UserService.Services.Implementations.UserService(repo.Object, banService.Object, mapper, logger, picFactory.Object);
    }

    [Fact]
    public async Task GetById_Returns_Mapped_Dto_Or_Null()
    {
        var service = CreateService(out var repo, out var banSvc);
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1", Name = "A" });
        repo.Setup(r => r.GetByIdAsync("u2")).ReturnsAsync((User?)null);

        var a = await service.GetByIdAsync("u1");
        var b = await service.GetByIdAsync("u2");
        a!.Id.Should().Be("u1");
        b.Should().BeNull();
    }

    [Fact]
    public async Task Create_Validates_Uniqueness_And_Sets_Id()
    {
        var service = CreateService(out var repo, out var banSvc);
        repo.Setup(r => r.ExistsByEmailAsync("a@x.com")).ReturnsAsync(false);
        repo.Setup(r => r.ExistsByProviderAndProviderUserIdAsync("github", "p1")).ReturnsAsync(false);
        repo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var dto = new CreateUserDto { Name = "A", Email = "a@x.com", Provider = "github", ProviderUserId = "p1" };
        var created = await service.CreateAsync(dto);
        created.Id.Should().NotBeNullOrEmpty();
        created.Email.Should().Be("a@x.com");
    }

    [Fact]
    public async Task Create_Throws_On_Duplicate_Email_Or_ProviderId()
    {
        var service = CreateService(out var repo, out var banSvc);
        repo.Setup(r => r.ExistsByEmailAsync("a@x.com")).ReturnsAsync(true);
        var dto = new CreateUserDto { Email = "a@x.com", Provider = "github", ProviderUserId = "p1" };
        await FluentActions.Invoking(() => service.CreateAsync(dto)).Should().ThrowAsync<ArgumentException>();

        repo.Reset();
        repo.Setup(r => r.ExistsByEmailAsync("a@x.com")).ReturnsAsync(false);
        repo.Setup(r => r.ExistsByProviderAndProviderUserIdAsync("github", "p1")).ReturnsAsync(true);
        await FluentActions.Invoking(() => service.CreateAsync(dto)).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Update_Validates_Email_Uniqueness_And_Maps()
    {
        var service = CreateService(out var repo, out var banSvc);
        var existing = new User { Id = "u1", Email = "old@x.com" };
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(existing);
        repo.Setup(r => r.ExistsByEmailAsync("new@x.com")).ReturnsAsync(false);
        repo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);

        var dto = new UpdateUserDto { Email = "new@x.com", Name = "New" };
        var updated = await service.UpdateAsync("u1", dto);
        updated.Email.Should().Be("new@x.com");
        updated.Name.Should().Be("New");
    }

    [Fact]
    public async Task Delete_Calls_Repo_When_Found()
    {
        var service = CreateService(out var repo, out var banSvc);
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1" });
        repo.Setup(r => r.DeleteAsync("u1")).Returns(Task.CompletedTask);
        await service.DeleteAsync("u1");
    }

    [Fact]
    public async Task OAuth_CreateOrUpdate_Creates_Or_Updates_As_Expected()
    {
        var service = CreateService(out var repo, out var banSvc);
        // Case 1: existing by provider
        var existing = new User { Id = "x", Provider = "github", ProviderUserId = "p1" };
        repo.Setup(r => r.GetByProviderAndProviderUserIdAsync("github", "p1")).ReturnsAsync(existing);
        repo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);
        var dto1 = await service.CreateOrUpdateFromOAuthAsync(new UserService.Models.Authentication.OAuthUserInfo { Id = "p1", Name = "A", Email = "a@x.com" }, "github");
        dto1.Should().NotBeNull();

        // Case 2: existing by email
        repo.Reset();
        var byEmail = new User { Id = "y", Provider = "google", ProviderUserId = "q" };
        repo.Setup(r => r.GetByProviderAndProviderUserIdAsync("github", "p2")).ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByEmailAsync("b@x.com")).ReturnsAsync(byEmail);
        repo.Setup(r => r.UpdateAsync(byEmail)).ReturnsAsync(byEmail);
        var dto2 = await service.CreateOrUpdateFromOAuthAsync(new UserService.Models.Authentication.OAuthUserInfo { Id = "p2", Name = "B", Email = "b@x.com" }, "github");
        dto2.Should().NotBeNull();

        // Case 3: create new
        repo.Reset();
        repo.Setup(r => r.GetByProviderAndProviderUserIdAsync("github", "p3")).ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByEmailAsync("c@x.com")).ReturnsAsync((User?)null);
        repo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        var dto3 = await service.CreateOrUpdateFromOAuthAsync(new UserService.Models.Authentication.OAuthUserInfo { Id = "p3", Name = "C", Email = "c@x.com", Picture = "img" }, "github");
        dto3.Should().NotBeNull();
    }

    [Fact]
    public void Helpers_CreateUserFromOAuth_And_Id_Generation_Work()
    {
        var service = CreateService(out var repo, out var banSvc);
        var user = service.CreateUserFromOAuth(new UserService.Models.Authentication.OAuthUserInfo { Id = "p1", Name = "A", Email = "a@x.com" }, "github");
        user.Id.Should().NotBeNullOrEmpty();
        service.ValidateUserId(user.Id, "github", "p1").Should().BeTrue();
    }

    [Fact]
    public async Task Http_Wrappers_Return_Correct_ActionResults()
    {
        var service = CreateService(out var repo, out var banSvc);
        var http = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();

        // GetById
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1" });
        (await service.GetByIdAsync(http, "u1")).Should().BeOfType<OkObjectResult>();
        repo.Setup(r => r.GetByIdAsync("miss")).ReturnsAsync((User?)null);
        (await service.GetByIdAsync(http, "miss")).Should().BeOfType<NotFoundObjectResult>();

        // GetByEmail
        repo.Setup(r => r.GetByEmailAsync("a@x.com")).ReturnsAsync(new User { Id = "u1" });
        (await service.GetByEmailAsync(http, "a@x.com")).Should().BeOfType<OkObjectResult>();
        repo.Setup(r => r.GetByEmailAsync("b@x.com")).ReturnsAsync((User?)null);
        (await service.GetByEmailAsync(http, "b@x.com")).Should().BeOfType<NotFoundObjectResult>();

        // GetAll / Search
        repo.Setup(r => r.GetAllWithCountAsync(1, 10)).ReturnsAsync((new List<User> { new User{ Id="x" } }, 1));
        (await service.GetAllAsync(http, 1, 10)).Should().BeOfType<OkObjectResult>();
        repo.Setup(r => r.SearchUsersAsync("al", 1, 10)).ReturnsAsync((new List<User> { new User{ Id="x" } }, 1));
        (await service.SearchUsersAsync(http, "al", 1, 10)).Should().BeOfType<OkObjectResult>();

        // GetUsersByIds
        repo.Setup(r => r.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<User> { new User{ Id="x" } });
        (await service.GetUsersByIdsAsync(http, new[] { "x" })).Should().BeOfType<OkObjectResult>();

        // Create / Update / Delete
        var create = new CreateUserDto { Email = "a@x.com", Provider = "github", ProviderUserId = "p1" };
        repo.Setup(r => r.ExistsByEmailAsync("a@x.com")).ReturnsAsync(false);
        repo.Setup(r => r.ExistsByProviderAndProviderUserIdAsync("github", "p1")).ReturnsAsync(false);
        repo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        (await service.CreateAsync(http, create, modelState)).Should().BeOfType<CreatedAtActionResult>();

        modelState.AddModelError("email", "required");
        (await service.CreateAsync(http, create, modelState)).Should().BeOfType<BadRequestObjectResult>();
        modelState.Clear();

        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1" });
        repo.Setup(r => r.ExistsByEmailAsync("n@x.com")).ReturnsAsync(false);
        repo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        (await service.UpdateAsync(http, "u1", new UpdateUserDto { Email = "n@x.com" }, modelState)).Should().BeOfType<OkObjectResult>();

        // For delete wrapper, simulate repo throwing to test NotFound and success path
        repo.Setup(r => r.GetByIdAsync("miss")).ReturnsAsync((User?)null);
        (await service.DeleteAsync(http, "miss")).Should().BeOfType<NotFoundObjectResult>();
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1" });
        repo.Setup(r => r.DeleteAsync("u1")).Returns(Task.CompletedTask);
        (await service.DeleteAsync(http, "u1")).Should().BeOfType<NoContentResult>();
    }
}


