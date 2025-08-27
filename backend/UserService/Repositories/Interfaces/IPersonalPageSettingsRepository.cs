using UserService.Models.Entities;
using UserService.Models.Requests;

namespace UserService.Repositories.Interfaces;

public interface IPersonalPageSettingsRepository
{
    Task<PersonalPageSettings?> GetByUserIdAsync(string userId);
    Task<PersonalPageSettings?> GetByPageSlugAsync(string pageSlug);
    Task<PersonalPageSettings> CreateAsync(PersonalPageSettings settings);
    Task<PersonalPageSettings> UpdateAsync(PersonalPageSettings settings);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string userId);
    Task<bool> PageSlugExistsAsync(string pageSlug, string? excludeUserId = null);
    Task<(List<PersonalPageSettings> settings, int totalCount)> GetPublicPaginatedAsync(PortfolioQueryParameters parameters);
} 