using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class BlogImageService : IBlogImageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<BlogImageService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB for blog images (larger than profile pictures)

    public BlogImageService(IWebHostEnvironment environment, ILogger<BlogImageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveBlogImageAsync(IFormFile file, string userId)
    {
        try
        {
            if (!IsValidImageFile(file))
            {
                throw new ArgumentException("Invalid file format or size");
            }

            // Create directory if it doesn't exist
            var uploadsDir = Path.Combine(_environment.ContentRootPath, "assets", "blog-images");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Generate unique filename with timestamp for blog images
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{userId}_{timestamp}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            var relativePath = Path.Combine("assets", "blog-images", fileName).Replace("\\", "/");
            
            _logger.LogInformation("Blog image saved: {FilePath} for user {UserId}", relativePath, userId);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving blog image for user {UserId}", userId);
            throw;
        }
    }

    public Task DeleteBlogImageAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Task.CompletedTask;

            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Blog image deleted: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog image: {FilePath}", filePath);
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

    public async Task<(byte[] fileBytes, string contentType, string fileName)?> GetBlogImageAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Blog image not found: {FilePath}", filePath);
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

            _logger.LogDebug("Blog image retrieved: {FilePath}", filePath);
            return (fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blog image: {FilePath}", filePath);
            return null;
        }
    }
}

