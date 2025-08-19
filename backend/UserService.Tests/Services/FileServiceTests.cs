using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using UserService.Services.Implementations;

namespace UserService.Tests.Services;

public class FileServiceTests
{
    [Fact]
    public async Task Save_And_Get_And_Delete_Profile_Picture_Workflow()
    {
        var env = new Mock<IWebHostEnvironment>();
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);
        env.Setup(e => e.ContentRootPath).Returns(root);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FileService>>().Object;
        var svc = new FileService(env.Object, logger);

        var fileBytes = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(fileBytes);
        var file = new FormFile(stream, 0, fileBytes.Length, "data", "pic.jpg");

        var rel = await svc.SaveProfilePictureAsync(file, "u1");
        rel.Should().Contain("assets/profile-pictures/");

        var got = await svc.GetProfilePictureAsync(rel);
        got.Should().NotBeNull();
        got!.Value.fileBytes.Should().Equal(fileBytes);
        got.Value.contentType.Should().Be("image/jpeg");

        await svc.DeleteProfilePictureAsync(rel);
        Directory.Delete(root, true);
    }

    [Fact]
    public void IsValidImageFile_Validates_Size_And_Extension()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FileService>>().Object;
        var svc = new FileService(env.Object, logger);

        var bytes = new byte[10];
        var stream = new MemoryStream(bytes);
        var valid = new FormFile(stream, 0, bytes.Length, "data", "a.png");
        svc.IsValidImageFile(valid).Should().BeTrue();

        var invalidExt = new FormFile(stream, 0, bytes.Length, "data", "a.exe");
        svc.IsValidImageFile(invalidExt).Should().BeFalse();

        var zero = new FormFile(new MemoryStream(), 0, 0, "data", "a.png");
        svc.IsValidImageFile(zero).Should().BeFalse();
    }

    [Fact]
    public async Task GetProfilePictureAsync_Returns_Null_When_Missing()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FileService>>().Object;
        var svc = new FileService(env.Object, logger);
        var missing = await svc.GetProfilePictureAsync("assets/profile-pictures/__missing__.png");
        missing.Should().BeNull();
    }
}


