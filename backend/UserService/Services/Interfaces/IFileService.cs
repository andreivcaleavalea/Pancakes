namespace UserService.Services.Interfaces;

public interface IFileService
{
    Task<string> SaveProfilePictureAsync(IFormFile file, string userId);
    Task DeleteProfilePictureAsync(string filePath);
    bool IsValidImageFile(IFormFile file);
    Task<(byte[] fileBytes, string contentType, string fileName)?> GetProfilePictureAsync(string filePath);
} 