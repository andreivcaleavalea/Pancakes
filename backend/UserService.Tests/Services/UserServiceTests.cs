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
    public async Task GetByIdAsync_Should_ReturnMappedDto_WhenFound_Or_Null_WhenMissing()
    {
        // Arrange
        var service = CreateService(out var repo, out var banSvc);
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1", Name = "A" });
        repo.Setup(r => r.GetByIdAsync("u2")).ReturnsAsync((User?)null);

        // Act
        var a = await service.GetByIdAsync("u1");
        var b = await service.GetByIdAsync("u2");

        // Assert
        a!.Id.Should().Be("u1");
        b.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_ValidateUniqueness_And_SetId()
    {
        // Arrange
        var service = CreateService(out var repo, out var banSvc);
        repo.Setup(r => r.ExistsByEmailAsync("a@x.com")).ReturnsAsync(false);
        repo.Setup(r => r.ExistsByProviderAndProviderUserIdAsync("github", "p1")).ReturnsAsync(false);
        repo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var dto = new CreateUserDto { Name = "A", Email = "a@x.com", Provider = "github", ProviderUserId = "p1" };
        var created = await service.CreateAsync(dto);

        // Assert
        created.Id.Should().NotBeNullOrEmpty();
        created.Email.Should().Be("a@x.com");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_DuplicateEmail_Or_ProviderId()
    {
        // Arrange
        var service = CreateService(out var repo, out var banSvc);
        repo.Setup(r => r.ExistsByEmailAsync("a@x.com")).ReturnsAsync(true);
        var dto = new CreateUserDto { Email = "a@x.com", Provider = "github", ProviderUserId = "p1" };

        // Act & Assert
        await FluentActions.Awaiting(() => service.CreateAsync(dto)).Should().ThrowAsync<ArgumentException>();

        // Arrange (duplicate provider id)
        repo.Reset();
        repo.Setup(r => r.ExistsByEmailAsync("a@x.com")).ReturnsAsync(false);
        repo.Setup(r => r.ExistsByProviderAndProviderUserIdAsync("github", "p1")).ReturnsAsync(true);

        // Act & Assert
        await FluentActions.Awaiting(() => service.CreateAsync(dto)).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_Should_ValidateEmailUniqueness_And_MapChanges()
    {
        // Arrange
        var service = CreateService(out var repo, out var banSvc);
        var existing = new User { Id = "u1", Email = "old@x.com" };
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(existing);
        repo.Setup(r => r.ExistsByEmailAsync("new@x.com")).ReturnsAsync(false);
        repo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);

        // Act
        var dto = new UpdateUserDto { Email = "new@x.com", Name = "New" };
        var updated = await service.UpdateAsync("u1", dto);

        // Assert
        updated.Email.Should().Be("new@x.com");
        updated.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_WhenUserExists()
    {
        // Arrange
        var service = CreateService(out var repo, out var banSvc);
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1" });
        repo.Setup(r => r.DeleteAsync("u1")).Returns(Task.CompletedTask);

        // Act
        await service.DeleteAsync("u1");

        // Assert (no exception thrown implies success)
    }

    [Fact]
    public async Task CreateOrUpdateFromOAuthAsync_Should_Create_Or_Update_AsExpected()
    {
        // Arrange
        var service = CreateService(out var repo, out var banSvc);
        // Case 1: existing by provider
        var existing = new User { Id = "x", Provider = "github", ProviderUserId = "p1" };
        repo.Setup(r => r.GetByProviderAndProviderUserIdAsync("github", "p1")).ReturnsAsync(existing);
        repo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);
        // Act
        var dto1 = await service.CreateOrUpdateFromOAuthAsync(new UserService.Models.Authentication.OAuthUserInfo { Id = "p1", Name = "A", Email = "a@x.com" }, "github");
        // Assert
        dto1.Should().NotBeNull();

        // Case 2: existing by email
        repo.Reset();
        var byEmail = new User { Id = "y", Provider = "google", ProviderUserId = "q" };
        repo.Setup(r => r.GetByProviderAndProviderUserIdAsync("github", "p2")).ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByEmailAsync("b@x.com")).ReturnsAsync(byEmail);
        repo.Setup(r => r.UpdateAsync(byEmail)).ReturnsAsync(byEmail);
        // Act
        var dto2 = await service.CreateOrUpdateFromOAuthAsync(new UserService.Models.Authentication.OAuthUserInfo { Id = "p2", Name = "B", Email = "b@x.com" }, "github");
        // Assert
        dto2.Should().NotBeNull();

        // Case 3: create new
        repo.Reset();
        repo.Setup(r => r.GetByProviderAndProviderUserIdAsync("github", "p3")).ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByEmailAsync("c@x.com")).ReturnsAsync((User?)null);
        repo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        // Act
        var dto3 = await service.CreateOrUpdateFromOAuthAsync(new UserService.Models.Authentication.OAuthUserInfo { Id = "p3", Name = "C", Email = "c@x.com", Picture = "img" }, "github");
        // Assert
        dto3.Should().NotBeNull();
    }

    [Fact]
    public void Helpers_Should_CreateUserFromOAuth_And_ValidateId()
    {
        // Arrange
        var service = CreateService(out var repo, out var banSvc);
        // Act
        var user = service.CreateUserFromOAuth(new UserService.Models.Authentication.OAuthUserInfo { Id = "p1", Name = "A", Email = "a@x.com" }, "github");
        // Assert
        user.Id.Should().NotBeNullOrEmpty();
        service.ValidateUserId(user.Id, "github", "p1").Should().BeTrue();
    }

    [Fact]
    public async Task Http_Wrappers_Should_Return_Correct_ActionResults()
    {
        // Arrange
        var service = CreateService(out var repo, out var banSvc);
        var http = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();

        // Act & Assert - GetById
        repo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1" });
        (await service.GetByIdAsync(http, "u1")).Should().BeOfType<OkObjectResult>();
        repo.Setup(r => r.GetByIdAsync("miss")).ReturnsAsync((User?)null);
        (await service.GetByIdAsync(http, "miss")).Should().BeOfType<NotFoundObjectResult>();

        // Act & Assert - GetByEmail
        repo.Setup(r => r.GetByEmailAsync("a@x.com")).ReturnsAsync(new User { Id = "u1" });
        (await service.GetByEmailAsync(http, "a@x.com")).Should().BeOfType<OkObjectResult>();
        repo.Setup(r => r.GetByEmailAsync("b@x.com")).ReturnsAsync((User?)null);
        (await service.GetByEmailAsync(http, "b@x.com")).Should().BeOfType<NotFoundObjectResult>();

        // Act & Assert - GetAll / Search
        repo.Setup(r => r.GetAllWithCountAsync(1, 10)).ReturnsAsync((new List<User> { new User{ Id="x" } }, 1));
        (await service.GetAllAsync(http, 1, 10)).Should().BeOfType<OkObjectResult>();
        repo.Setup(r => r.SearchUsersAsync("al", 1, 10)).ReturnsAsync((new List<User> { new User{ Id="x" } }, 1));
        (await service.SearchUsersAsync(http, "al", 1, 10)).Should().BeOfType<OkObjectResult>();

        // Act & Assert - GetUsersByIds
        repo.Setup(r => r.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<User> { new User{ Id="x" } });
        (await service.GetUsersByIdsAsync(http, new[] { "x" })).Should().BeOfType<OkObjectResult>();

        // Act & Assert - Create / Update / Delete
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


