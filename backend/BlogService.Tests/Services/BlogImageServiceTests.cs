using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Services;

public class BlogImageServiceTests
{
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), "BlogImageServiceTests", Guid.NewGuid().ToString());
    
    private BlogImageService CreateService()
    {
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(e => e.ContentRootPath).Returns(_testDirectory);
        
        var logger = new Mock<ILogger<BlogImageService>>();
        
        return new BlogImageService(mockEnvironment.Object, logger.Object);
    }

    private static Mock<IFormFile> CreateMockFormFile(
        string fileName = "test.jpg", 
        long length = 1024, 
        string contentType = "image/jpeg",
        byte[]? content = null)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        
        if (content != null)
        {
            var stream = new MemoryStream(content);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((target, _) => 
                {
                    stream.Position = 0;
                    stream.CopyTo(target);
                });
        }
        else
        {
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
        
        return mockFile;
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public void IsValidImageFile_ValidFile_ReturnsTrue()
    {
        var service = CreateService();
        var mockFile = CreateMockFormFile("image.jpg", 1024);

        var result = service.IsValidImageFile(mockFile.Object);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidImageFile_EmptyFile_ReturnsFalse()
    {
        var service = CreateService();
        var mockFile = CreateMockFormFile("image.jpg", 0);

        var result = service.IsValidImageFile(mockFile.Object);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_FileTooLarge_ReturnsFalse()
    {
        var service = CreateService();
        var mockFile = CreateMockFormFile("image.jpg", 11 * 1024 * 1024); // 11MB

        var result = service.IsValidImageFile(mockFile.Object);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("image.jpg")]
    [InlineData("image.jpeg")]
    [InlineData("image.png")]
    [InlineData("image.gif")]
    [InlineData("image.webp")]
    [InlineData("IMAGE.JPG")]
    [InlineData("IMAGE.JPEG")]
    public void IsValidImageFile_ValidExtensions_ReturnsTrue(string fileName)
    {
        var service = CreateService();
        var mockFile = CreateMockFormFile(fileName, 1024);

        var result = service.IsValidImageFile(mockFile.Object);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("script.exe")]
    [InlineData("data.txt")]
    [InlineData("archive.zip")]
    [InlineData("image.bmp")]
    public void IsValidImageFile_InvalidExtensions_ReturnsFalse(string fileName)
    {
        var service = CreateService();
        var mockFile = CreateMockFormFile(fileName, 1024);

        var result = service.IsValidImageFile(mockFile.Object);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveBlogImageAsync_InvalidFile_ThrowsArgumentException()
    {
        var service = CreateService();
        var mockFile = CreateMockFormFile("document.txt", 1024);

        Func<Task> act = async () => await service.SaveBlogImageAsync(mockFile.Object, "user123");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid file format or size");
    }

    [Fact]
    public async Task SaveBlogImageAsync_ValidFile_CreatesDirectoryAndSavesFile()
    {
        var service = CreateService();
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var mockFile = CreateMockFormFile("test.jpg", fileContent.Length, "image/jpeg", fileContent);

        var result = await service.SaveBlogImageAsync(mockFile.Object, "user123");

        result.Should().StartWith("assets/blog-images/user123_");
        result.Should().EndWith(".jpg");
        result.Should().Contain("_"); // Should contain timestamp and GUID

        // Verify directory was created
        var expectedDirectory = Path.Combine(_testDirectory, "assets", "blog-images");
        Directory.Exists(expectedDirectory).Should().BeTrue();

        // Verify file was saved
        var fileName = Path.GetFileName(result.Replace("/", Path.DirectorySeparatorChar.ToString()));
        var filePath = Path.Combine(expectedDirectory, fileName);
        File.Exists(filePath).Should().BeTrue();

        // Verify file content
        var savedContent = await File.ReadAllBytesAsync(filePath);
        savedContent.Should().BeEquivalentTo(fileContent);
    }

    [Fact]
    public async Task SaveBlogImageAsync_FileNameFormat_IsCorrect()
    {
        var service = CreateService();
        var mockFile = CreateMockFormFile("original.png", 1024, "image/png");

        var result = await service.SaveBlogImageAsync(mockFile.Object, "user456");

        // Format should be: user456_yyyyMMdd_HHmmss_guid.png
        var fileName = Path.GetFileName(result);
        fileName.Should().StartWith("user456_");
        fileName.Should().EndWith(".png");
        
        var parts = fileName.Split('_');
        parts.Should().HaveCountGreaterThan(2);
        parts[0].Should().Be("user456");
        
        // Verify timestamp format (yyyyMMdd_HHmmss)
        parts[1].Should().MatchRegex(@"^\d{8}$"); // Date part
        parts[2].Should().MatchRegex(@"^\d{6}$"); // Time part
    }

    [Fact]
    public async Task GetBlogImageAsync_FileNotExists_ReturnsNull()
    {
        var service = CreateService();

        var result = await service.GetBlogImageAsync("assets/blog-images/nonexistent.jpg");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBlogImageAsync_EmptyFilePath_ReturnsNull()
    {
        var service = CreateService();

        var result = await service.GetBlogImageAsync(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBlogImageAsync_NullFilePath_ReturnsNull()
    {
        var service = CreateService();

        var result = await service.GetBlogImageAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBlogImageAsync_ExistingFile_ReturnsCorrectData()
    {
        var service = CreateService();
        var fileContent = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header bytes
        
        // Create the directory and file
        var directory = Path.Combine(_testDirectory, "assets", "blog-images");
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, "test.jpg");
        await File.WriteAllBytesAsync(filePath, fileContent);

        var result = await service.GetBlogImageAsync("assets/blog-images/test.jpg");

        result.Should().NotBeNull();
        result!.Value.fileBytes.Should().BeEquivalentTo(fileContent);
        result!.Value.contentType.Should().Be("image/jpeg");
        result!.Value.fileName.Should().Be("test.jpg");
    }

    [Theory]
    [InlineData("test.jpg", "image/jpeg")]
    [InlineData("test.jpeg", "image/jpeg")]
    [InlineData("test.png", "image/png")]
    [InlineData("test.gif", "image/gif")]
    [InlineData("test.webp", "image/webp")]
    [InlineData("test.unknown", "application/octet-stream")]
    public async Task GetBlogImageAsync_DifferentExtensions_ReturnsCorrectContentType(string fileName, string expectedContentType)
    {
        var service = CreateService();
        var fileContent = new byte[] { 1, 2, 3, 4 };
        
        var directory = Path.Combine(_testDirectory, "assets", "blog-images");
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(filePath, fileContent);

        var result = await service.GetBlogImageAsync($"assets/blog-images/{fileName}");

        result.Should().NotBeNull();
        result!.Value.contentType.Should().Be(expectedContentType);
    }

    [Fact]
    public async Task DeleteBlogImageAsync_EmptyFilePath_CompletesSuccessfully()
    {
        var service = CreateService();

        // Should not throw
        await service.DeleteBlogImageAsync(string.Empty);
    }

    [Fact]
    public async Task DeleteBlogImageAsync_NullFilePath_CompletesSuccessfully()
    {
        var service = CreateService();

        // Should not throw
        await service.DeleteBlogImageAsync(null!);
    }

    [Fact]
    public async Task DeleteBlogImageAsync_FileNotExists_CompletesSuccessfully()
    {
        var service = CreateService();

        // Should not throw
        await service.DeleteBlogImageAsync("assets/blog-images/nonexistent.jpg");
    }

    [Fact]
    public async Task DeleteBlogImageAsync_ExistingFile_DeletesFile()
    {
        var service = CreateService();
        
        // Create the directory and file
        var directory = Path.Combine(_testDirectory, "assets", "blog-images");
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, "test.jpg");
        await File.WriteAllBytesAsync(filePath, new byte[] { 1, 2, 3 });

        // Verify file exists
        File.Exists(filePath).Should().BeTrue();

        await service.DeleteBlogImageAsync("assets/blog-images/test.jpg");

        // Verify file was deleted
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteBlogImageAsync_FileInUse_HandlesExceptionGracefully()
    {
        var service = CreateService();
        
        var directory = Path.Combine(_testDirectory, "assets", "blog-images");
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, "test.jpg");
        await File.WriteAllBytesAsync(filePath, new byte[] { 1, 2, 3 });

        // Open file to simulate it being in use
        using var fileStream = File.OpenRead(filePath);
        
        // Should not throw, just handle gracefully
        await service.DeleteBlogImageAsync("assets/blog-images/test.jpg");
        
        // File should still exist since it was in use
        File.Exists(filePath).Should().BeTrue();
    }
}
