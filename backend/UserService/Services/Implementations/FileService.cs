using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveProfilePictureAsync(IFormFile file, string userId)
    {
        try
        {
            if (!IsValidImageFile(file))
            {
                throw new ArgumentException("Invalid file format or size");
            }

            // Create directory if it doesn't exist
            var uploadsDir = Path.Combine(_environment.ContentRootPath, "assets", "profile-pictures");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            var relativePath = Path.Combine("assets", "profile-pictures", fileName).Replace("\\", "/");
            
            _logger.LogInformation("Profile picture saved: {FilePath} for user {UserId}", relativePath, userId);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving profile picture for user {UserId}", userId);
            throw;
        }
    }

    public Task DeleteProfilePictureAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Task.CompletedTask;

            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Profile picture deleted: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile picture: {FilePath}", filePath);
        }
        
        return Task.CompletedTask;
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file.Length == 0)
            return false;

        if (file.Length > MaxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return false;

        return true;
    }

    public async Task<(byte[] fileBytes, string contentType, string fileName)?> GetProfilePictureAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Profile picture not found: {FilePath}", filePath);
                return null;
            }

            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            var fileName = Path.GetFileName(fullPath);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            _logger.LogDebug("Profile picture retrieved: {FilePath}", filePath);
            return (fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile picture: {FilePath}", filePath);
            return null;
        }
    }
} 