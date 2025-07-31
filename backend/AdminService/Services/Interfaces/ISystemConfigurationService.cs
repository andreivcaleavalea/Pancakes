using AdminService.Models.Entities;
using AdminService.Models.Responses;

namespace AdminService.Services.Interfaces
{
    public interface ISystemConfigurationService
    {
        Task<List<SystemConfiguration>> GetAllConfigurationsAsync();
        Task<List<SystemConfiguration>> GetConfigurationsByCategoryAsync(string category);
        Task<SystemConfiguration?> GetConfigurationAsync(string key);
        Task<T?> GetConfigurationValueAsync<T>(string key);
        Task<SystemConfiguration> SetConfigurationAsync(string key, string value, string category, string description, string dataType, string updatedBy);
        Task<bool> DeleteConfigurationAsync(string key);
        Task<bool> IsMaintenanceModeAsync();
        Task<bool> SetMaintenanceModeAsync(bool enabled, string updatedBy);
        Task<Dictionary<string, object>> GetPublicConfigurationsAsync();
        Task<bool> ValidateConfigurationValueAsync(string key, string value, string dataType);
    }
}