using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Controllers;
using UserService.Services.Interfaces;

namespace UserService.Tests.Controllers;

public class AssetsControllerTests
{
    [Fact]
    public async Task GetProfilePicture_Returns_File_When_Found()
    {
        var file = new Mock<IFileService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AssetsController>>().Object;
        var ctrl = new AssetsController(file.Object, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        file.Setup(f => f.GetProfilePictureAsync(It.IsAny<string>()))
            .ReturnsAsync((new byte[] { 1, 2 }, "image/png", "a.png"));

        var result = await ctrl.GetProfilePicture("a.png");
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task GetProfilePicture_Returns_NotFound_When_Missing()
    {
        var file = new Mock<IFileService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AssetsController>>().Object;
        var ctrl = new AssetsController(file.Object, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        file.Setup(f => f.GetProfilePictureAsync(It.IsAny<string>()))
            .ReturnsAsync(((byte[], string, string)?)null);

        var result = await ctrl.GetProfilePicture("missing.png");
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetProfilePicture_BadRequest_On_Empty_Or_Invalid_Name()
    {
        var file = new Mock<IFileService>(MockBehavior.Loose);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AssetsController>>().Object;
        var ctrl = new AssetsController(file.Object, logger) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        (await ctrl.GetProfilePicture(string.Empty)).Should().BeOfType<BadRequestObjectResult>();
        (await ctrl.GetProfilePicture("../evil.png")).Should().BeOfType<BadRequestObjectResult>();
        (await ctrl.GetProfilePicture("a/b.png")).Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetProfilePicture_Returns_500_On_Exception()
    {
        var file = new Mock<IFileService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AssetsController>>().Object;
        var ctrl = new AssetsController(file.Object, logger) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        file.Setup(f => f.GetProfilePictureAsync(It.IsAny<string>())).ThrowsAsync(new Exception("boom"));
        var result = await ctrl.GetProfilePicture("a.png");
        result.Should().BeOfType<ObjectResult>().Which.As<ObjectResult>().StatusCode.Should().Be(500);
    }
}


