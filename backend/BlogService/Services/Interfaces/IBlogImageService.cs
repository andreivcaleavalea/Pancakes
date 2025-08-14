using Microsoft.AspNetCore.Http;

namespace BlogService.Services.Interfaces;

public interface IBlogImageService
{
    Task<string> SaveBlogImageAsync(IFormFile file, string userId);
    Task DeleteBlogImageAsync(string filePath);
    bool IsValidImageFile(IFormFile file);
    Task<(byte[] fileBytes, string contentType, string fileName)?> GetBlogImageAsync(string filePath);
}

