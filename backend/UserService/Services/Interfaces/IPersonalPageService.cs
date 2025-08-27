using UserService.Models.DTOs;
using UserService.Models.Requests;

namespace UserService.Services.Interfaces;

public interface IPersonalPageService
{
    Task<PersonalPageSettingsDto> GetSettingsAsync(string userId);
    Task<PersonalPageSettingsDto> UpdateSettingsAsync(string userId, UpdatePersonalPageSettingsDto updateDto);
    Task<PublicPersonalPageDto?> GetPublicPageAsync(string pageSlug);
    Task<string> GenerateUniqueSlugAsync(string baseName, string userId);
    Task<PaginatedResult<PublicPersonalPageDto>> GetPublicPortfoliosAsync(PortfolioQueryParameters parameters);
} 