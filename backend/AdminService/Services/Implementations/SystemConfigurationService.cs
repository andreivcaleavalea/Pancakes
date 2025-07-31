using AdminService.Data;
using AdminService.Models.Entities;
using AdminService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AdminService.Services.Implementations
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly AdminDbContext _context;
        private readonly ILogger<SystemConfigurationService> _logger;

        public SystemConfigurationService(AdminDbContext context, ILogger<SystemConfigurationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SystemConfiguration>> GetAllConfigurationsAsync()
        {
            return await _context.SystemConfigurations.ToListAsync();
        }

        public async Task<List<SystemConfiguration>> GetConfigurationsByCategoryAsync(string category)
        {
            return await _context.SystemConfigurations
                .Where(c => c.Category == category)
                .ToListAsync();
        }

        public async Task<SystemConfiguration?> GetConfigurationAsync(string key)
        {
            return await _context.SystemConfigurations
                .FirstOrDefaultAsync(c => c.Key == key);
        }

        public async Task<T?> GetConfigurationValueAsync<T>(string key)
        {
            var config = await GetConfigurationAsync(key);
            if (config == null)
                return default;

            try
            {
                return config.DataType switch
                {
                    "bool" => (T)(object)bool.Parse(config.Value),
                    "int" => (T)(object)int.Parse(config.Value),
                    "decimal" => (T)(object)decimal.Parse(config.Value),
                    "json" => JsonSerializer.Deserialize<T>(config.Value),
                    _ => (T)(object)config.Value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting configuration value for key {Key}", key);
                return default;
            }
        }

        public async Task<SystemConfiguration> SetConfigurationAsync(string key, string value, string category, string description, string dataType, string updatedBy)
        {
            var existing = await GetConfigurationAsync(key);
            
            if (existing != null)
            {
                existing.Value = value;
                existing.Category = category;
                existing.Description = description;
                existing.DataType = dataType;
                existing.UpdatedBy = updatedBy;
                existing.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return existing;
            }
            else
            {
                var newConfig = new SystemConfiguration
                {
                    Key = key,
                    Value = value,
                    Category = category,
                    Description = description,
                    DataType = dataType,
                    UpdatedBy = updatedBy
                };

                _context.SystemConfigurations.Add(newConfig);
                await _context.SaveChangesAsync();
                return newConfig;
            }
        }

        public async Task<bool> DeleteConfigurationAsync(string key)
        {
            var config = await GetConfigurationAsync(key);
            if (config == null)
                return false;

            _context.SystemConfigurations.Remove(config);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsMaintenanceModeAsync()
        {
            var maintenanceMode = await GetConfigurationValueAsync<bool>("MAINTENANCE_MODE");
            return maintenanceMode;
        }

        public async Task<bool> SetMaintenanceModeAsync(bool enabled, string updatedBy)
        {
            try
            {
                await SetConfigurationAsync(
                    "MAINTENANCE_MODE", 
                    enabled.ToString().ToLower(), 
                    "system", 
                    "Enable/disable maintenance mode for the entire platform", 
                    "bool", 
                    updatedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting maintenance mode to {Enabled}", enabled);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetPublicConfigurationsAsync()
        {
            var publicConfigs = await _context.SystemConfigurations
                .Where(c => !c.IsSecret)
                .ToListAsync();

            var result = new Dictionary<string, object>();
            
            foreach (var config in publicConfigs)
            {
                try
                {
                    var value = config.DataType switch
                    {
                        "bool" => bool.Parse(config.Value),
                        "int" => int.Parse(config.Value),
                        "decimal" => decimal.Parse(config.Value),
                        "json" => JsonSerializer.Deserialize<object>(config.Value),
                        _ => config.Value
                    };
                    
                    result[config.Key] = value ?? config.Value;
                }
                catch
                {
                    result[config.Key] = config.Value;
                }
            }

            return result;
        }

        public async Task<bool> ValidateConfigurationValueAsync(string key, string value, string dataType)
        {
            try
            {
                return dataType switch
                {
                    "bool" => bool.TryParse(value, out _),
                    "int" => int.TryParse(value, out _),
                    "decimal" => decimal.TryParse(value, out _),
                    "json" => IsValidJson(value),
                    _ => true // String values are always valid
                };
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidJson(string jsonString)
        {
            try
            {
                JsonDocument.Parse(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}