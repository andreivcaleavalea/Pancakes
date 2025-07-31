using AdminService.Models.Entities;
using AdminService.Models.Responses;
using AdminService.Models.DTOs;
using AdminService.Models.Requests;

namespace AdminService.Services.Interfaces
{
    public interface ISystemConfigurationService
    {
        Task<List<SystemConfiguration>> GetAllConfigurationsAsync();
        Task<List<SystemConfiguration>> GetConfigurationsByCategoryAsync(string category);
        Task<SystemConfiguration?> GetConfigurationAsync(string key);
        Task<SystemConfiguration?> GetConfigurationAsync(Guid id);
        Task<T?> GetConfigurationValueAsync<T>(string key);
        Task<SystemConfiguration> SetConfigurationAsync(string key, string value, string category, string description, string dataType, string updatedBy);
        Task<bool> DeleteConfigurationAsync(string key);
        Task<bool> DeleteConfigurationAsync(Guid id);
        Task<bool> IsMaintenanceModeAsync();
        Task<bool> SetMaintenanceModeAsync(bool enabled, string updatedBy);
        Task<Dictionary<string, object>> GetPublicConfigurationsAsync();
        Task<bool> ValidateConfigurationValueAsync(string key, string value, string dataType);
        
        // Methods used by the controller
        Task<PagedResponse<SystemConfigurationDto>> GetConfigurationsAsync(int page, int pageSize, string? category = null);
        Task<SystemConfigurationDto> CreateConfigurationAsync(CreateSystemConfigurationRequest request, string updatedBy);
        Task<SystemConfigurationDto> UpdateConfigurationAsync(Guid id, UpdateSystemConfigurationRequest request, string updatedBy);
    }
}