using BlogService.Controllers;
using BlogService.Models.DTOs;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BlogService.Tests.Controllers;

public class BlogImagesControllerTests
{
    private static BlogImagesController CreateController(
        out Mock<IBlogImageService> blogImageService,
        out Mock<IAuthorizationService> authService)
    {
        blogImageService = new Mock<IBlogImageService>(MockBehavior.Strict);
        authService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        var logger = new Mock<ILogger<BlogImagesController>>();
        
        var controller = new BlogImagesController(blogImageService.Object, authService.Object, logger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        
        return controller;
    }

    private static Mock<IFormFile> CreateMockFormFile(string fileName = "test.jpg", long length = 1024)
    {
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(length);
        formFile.Setup(f => f.ContentType).Returns("image/jpeg");
        return formFile;
    }

    [Fact]
    public async Task UploadBlogImage_NoImageFile_Returns_BadRequest()
    {
        var controller = CreateController(out _, out _);

        var result = await controller.UploadBlogImage(null!);
        
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("No image file provided");
    }

    [Fact]
    public async Task UploadBlogImage_UserNotAuthenticated_Returns_Unauthorized()
    {
        var controller = CreateController(out _, out var authService);
        var mockFile = CreateMockFormFile();

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync((UserInfoDto?)null);

        var result = await controller.UploadBlogImage(mockFile.Object);
        
        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("User not authenticated");
    }

    [Fact]
    public async Task UploadBlogImage_InvalidImageFile_Returns_BadRequest()
    {
        var controller = CreateController(out var blogImageService, out var authService);
        var mockFile = CreateMockFormFile();
        var currentUser = new UserInfoDto { Id = "user1", Name = "Test User" };

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync(currentUser);
        blogImageService.Setup(s => s.IsValidImageFile(mockFile.Object))
            .Returns(false);

        var result = await controller.UploadBlogImage(mockFile.Object);
        
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid image file. Supported formats: JPG, JPEG, PNG, GIF, WEBP. Maximum size: 10MB.");
    }

    [Fact]
    public async Task UploadBlogImage_ValidFile_Returns_Ok()
    {
        var controller = CreateController(out var blogImageService, out var authService);
        var mockFile = CreateMockFormFile();
        var currentUser = new UserInfoDto { Id = "user1", Name = "Test User" };
        const string savedPath = "assets/blog-images/user1_123456.jpg";

        // Set up request context for URL building
        controller.Request.Scheme = "https";
        controller.Request.Host = new HostString("localhost:5000");

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync(currentUser);
        blogImageService.Setup(s => s.IsValidImageFile(mockFile.Object))
            .Returns(true);
        blogImageService.Setup(s => s.SaveBlogImageAsync(mockFile.Object, currentUser.Id))
            .ReturnsAsync(savedPath);

        var result = await controller.UploadBlogImage(mockFile.Object);
        
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var responseData = okResult.Value;
        responseData.Should().NotBeNull();
        
        // Verify response structure
        var properties = responseData!.GetType().GetProperties();
        properties.Should().Contain(p => p.Name == "imagePath");
        properties.Should().Contain(p => p.Name == "imageUrl");
        properties.Should().Contain(p => p.Name == "message");
    }

    [Fact]
    public async Task UploadBlogImage_ArgumentException_Returns_BadRequest()
    {
        var controller = CreateController(out var blogImageService, out var authService);
        var mockFile = CreateMockFormFile();
        var currentUser = new UserInfoDto { Id = "user1", Name = "Test User" };

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync(currentUser);
        blogImageService.Setup(s => s.IsValidImageFile(mockFile.Object))
            .Returns(true);
        blogImageService.Setup(s => s.SaveBlogImageAsync(mockFile.Object, currentUser.Id))
            .ThrowsAsync(new ArgumentException("Invalid file"));

        var result = await controller.UploadBlogImage(mockFile.Object);
        
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid file");
    }

    [Fact]
    public async Task UploadBlogImage_GeneralException_Returns_500()
    {
        var controller = CreateController(out var blogImageService, out var authService);
        var mockFile = CreateMockFormFile();
        var currentUser = new UserInfoDto { Id = "user1", Name = "Test User" };

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync(currentUser);
        blogImageService.Setup(s => s.IsValidImageFile(mockFile.Object))
            .Returns(true);
        blogImageService.Setup(s => s.SaveBlogImageAsync(mockFile.Object, currentUser.Id))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await controller.UploadBlogImage(mockFile.Object);
        
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteBlogImage_EmptyFilename_Returns_BadRequest()
    {
        var controller = CreateController(out _, out _);

        var result = await controller.DeleteBlogImage(string.Empty);
        
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Filename is required");
    }

    [Fact]
    public async Task DeleteBlogImage_UserNotAuthenticated_Returns_Unauthorized()
    {
        var controller = CreateController(out _, out var authService);

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync((UserInfoDto?)null);

        var result = await controller.DeleteBlogImage("test.jpg");
        
        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("User not authenticated");
    }

    [Theory]
    [InlineData("../secret.txt")]
    [InlineData("folder/file.jpg")]
    [InlineData("folder\\file.jpg")]
    [InlineData("..\\secret.txt")]
    public async Task DeleteBlogImage_InvalidFilename_Returns_BadRequest(string filename)
    {
        var controller = CreateController(out _, out var authService);
        var currentUser = new UserInfoDto { Id = "user1", Name = "Test User" };

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync(currentUser);

        var result = await controller.DeleteBlogImage(filename);
        
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid filename");
    }

    [Fact]
    public async Task DeleteBlogImage_NotOwner_Returns_Forbid()
    {
        var controller = CreateController(out _, out var authService);
        var currentUser = new UserInfoDto { Id = "user1", Name = "Test User" };
        const string filename = "user2_123456.jpg"; // Different user's file

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync(currentUser);

        var result = await controller.DeleteBlogImage(filename);
        
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteBlogImage_ValidOwner_Returns_Ok()
    {
        var controller = CreateController(out var blogImageService, out var authService);
        var currentUser = new UserInfoDto { Id = "user1", Name = "Test User" };
        const string filename = "user1_123456.jpg"; // User's own file
        const string expectedPath = "assets/blog-images/user1_123456.jpg";

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync(currentUser);
        blogImageService.Setup(s => s.DeleteBlogImageAsync(expectedPath))
            .Returns(Task.CompletedTask);

        var result = await controller.DeleteBlogImage(filename);
        
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var responseData = okResult.Value;
        responseData.Should().NotBeNull();
        
        // Verify response structure
        var properties = responseData!.GetType().GetProperties();
        properties.Should().Contain(p => p.Name == "message");
    }

    [Fact]
    public async Task DeleteBlogImage_ServiceException_Returns_500()
    {
        var controller = CreateController(out var blogImageService, out var authService);
        var currentUser = new UserInfoDto { Id = "user1", Name = "Test User" };
        const string filename = "user1_123456.jpg";
        const string expectedPath = "assets/blog-images/user1_123456.jpg";

        authService.Setup(a => a.GetCurrentUserAsync(controller.HttpContext))
            .ReturnsAsync(currentUser);
        blogImageService.Setup(s => s.DeleteBlogImageAsync(expectedPath))
            .ThrowsAsync(new Exception("Delete failed"));

        var result = await controller.DeleteBlogImage(filename);
        
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
