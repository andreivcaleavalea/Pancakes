using UserService.Services.Implementations.ProfilePictureStrategies;
using UserService.Services.Interfaces;
using UserService.Models.Authentication;
using UserService.Models.Entities;

namespace UserService.Tests.Services;

public class ProfilePictureStrategiesTests
{
    [Fact]
    public void Factory_Selects_Strategies_And_Defaults()
    {
        var oauth = new OAuthProfilePictureStrategy(new Mock<Microsoft.Extensions.Logging.ILogger<OAuthProfilePictureStrategy>>().Object);
        var self = new SelfProvidedProfilePictureStrategy(new Mock<Microsoft.Extensions.Logging.ILogger<SelfProvidedProfilePictureStrategy>>().Object, new Mock<IFileService>().Object);
        var factory = new ProfilePictureStrategyFactory(new IProfilePictureStrategy[] { oauth, self }, new Mock<Microsoft.Extensions.Logging.ILogger<ProfilePictureStrategyFactory>>().Object);

        factory.GetStrategy("github").Should().BeOfType<OAuthProfilePictureStrategy>();
        factory.GetStrategy(null!).Should().BeOfType<OAuthProfilePictureStrategy>();
        factory.GetStrategy("unknown").Should().BeOfType<OAuthProfilePictureStrategy>();
        factory.GetStrategyForUserUpload().Should().BeOfType<SelfProvidedProfilePictureStrategy>();
    }

    [Fact]
    public void OAuthStrategy_Updates_When_Provider_Matches()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<OAuthProfilePictureStrategy>>().Object;
        var strat = new OAuthProfilePictureStrategy(logger);
        var user = new User { Id = "u1", Provider = "github" };
        var info = new OAuthUserInfo { Picture = "pic.png" };

        strat.CanHandle("github").Should().BeTrue();
        strat.ShouldUpdatePictureFromOAuth(user, info, "github").Should().BeTrue();
        user.Image.Should().Be("pic.png");
    }

    [Fact]
    public void OAuthStrategy_Does_Not_Update_When_User_Not_OAuth()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<OAuthProfilePictureStrategy>>().Object;
        var strat = new OAuthProfilePictureStrategy(logger);
        var user = new User { Id = "u1", Provider = "selfprovided", Image = "old.png" };
        var info = new OAuthUserInfo { Picture = "pic.png" };
        strat.ShouldUpdatePictureFromOAuth(user, info, "github").Should().BeFalse();
        user.Image.Should().Be("old.png");
    }

    [Fact]
    public void SelfProvidedStrategy_Preserves_Image_And_Sets_Provider_On_Upload()
    {
        var file = new Mock<IFileService>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<SelfProvidedProfilePictureStrategy>>().Object;
        var strat = new SelfProvidedProfilePictureStrategy(logger, file.Object);
        var user = new User { Id = "u1", Image = "assets/profile-pictures/old.png" };
        strat.ShouldUpdatePictureFromOAuth(user, new OAuthUserInfo { Picture = "x" }, "github").Should().BeFalse();
        strat.HandleUserUpload(user, "assets/profile-pictures/new.png");
        user.Image.Should().Contain("new.png");
        user.Provider.Should().Be("selfprovided");
        strat.CanHandle("selfprovided").Should().BeTrue();
        strat.GetProviderValue().Should().Be("selfprovided");
    }
}


