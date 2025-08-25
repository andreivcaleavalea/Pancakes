using BlogService.Controllers;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Controllers;

public class AssetsControllerTests
{
    private static AssetsController CreateController(out Mock<IBlogImageService> blogImageService)
    {
        blogImageService = new Mock<IBlogImageService>(MockBehavior.Strict);
        var logger = new Mock<ILogger<AssetsController>>();
        var controller = new AssetsController(blogImageService.Object, logger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    [Fact]
    public async Task GetBlogImage_EmptyFilename_Returns_BadRequest()
    {
        var controller = CreateController(out _);

        var result = await controller.GetBlogImage(string.Empty);
        
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Filename is required");
    }

    [Fact]
    public async Task GetBlogImage_NullFilename_Returns_BadRequest()
    {
        var controller = CreateController(out _);

        var result = await controller.GetBlogImage(null!);
        
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Filename is required");
    }

    [Theory]
    [InlineData("../secret.txt")]
    [InlineData("folder/file.jpg")]
    [InlineData("folder\\file.jpg")]
    [InlineData("..\\secret.txt")]
    public async Task GetBlogImage_InvalidFilename_Returns_BadRequest(string filename)
    {
        var controller = CreateController(out _);

        var result = await controller.GetBlogImage(filename);
        
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid filename");
    }

    [Fact]
    public async Task GetBlogImage_ValidFilename_NotFound_Returns_NotFound()
    {
        var controller = CreateController(out var blogImageService);
        const string filename = "image.jpg";
        const string expectedPath = "assets/blog-images/image.jpg";

        blogImageService.Setup(s => s.GetBlogImageAsync(expectedPath))
            .ReturnsAsync((byte[], string, string)?)null);

        var result = await controller.GetBlogImage(filename);
        
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Blog image not found");
    }

    [Fact]
    public async Task GetBlogImage_ValidFilename_Found_Returns_File()
    {
        var controller = CreateController(out var blogImageService);
        const string filename = "image.jpg";
        const string expectedPath = "assets/blog-images/image.jpg";
        var fileBytes = new byte[] { 1, 2, 3, 4 };
        const string contentType = "image/jpeg";
        const string fileName = "image.jpg";

        blogImageService.Setup(s => s.GetBlogImageAsync(expectedPath))
            .ReturnsAsync((fileBytes, contentType, fileName));

        var result = await controller.GetBlogImage(filename);
        
        result.Should().BeOfType<FileContentResult>();
        var fileResult = (FileContentResult)result;
        fileResult.FileContents.Should().BeEquivalentTo(fileBytes);
        fileResult.ContentType.Should().Be(contentType);
        fileResult.FileDownloadName.Should().Be(fileName);
        
        // Verify cache headers are set
        controller.Response.Headers["Cache-Control"].Should().Contain("public, max-age=86400");
        controller.Response.Headers["ETag"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetBlogImage_ServiceThrowsException_Returns_500()
    {
        var controller = CreateController(out var blogImageService);
        const string filename = "image.jpg";
        const string expectedPath = "assets/blog-images/image.jpg";

        blogImageService.Setup(s => s.GetBlogImageAsync(expectedPath))
            .ThrowsAsync(new Exception("Service error"));

        var result = await controller.GetBlogImage(filename);
        
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
        ((ObjectResult)result).Value.Should().Be("An error occurred while retrieving the blog image");
    }

    [Fact]
    public async Task GetBlogImage_SetsCorrectETagHeader()
    {
        var controller = CreateController(out var blogImageService);
        const string filename = "image.jpg";
        const string expectedPath = "assets/blog-images/image.jpg";
        var fileBytes = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in bytes
        const string contentType = "image/jpeg";
        const string fileName = "image.jpg";

        blogImageService.Setup(s => s.GetBlogImageAsync(expectedPath))
            .ReturnsAsync((fileBytes, contentType, fileName));

        await controller.GetBlogImage(filename);
        
        // Calculate expected MD5 hash
        var expectedHash = System.Security.Cryptography.MD5.HashData(fileBytes);
        var expectedETag = $"\"{Convert.ToHexString(expectedHash)}\"";
        
        controller.Response.Headers["ETag"].ToString().Should().Be(expectedETag);
    }
}
