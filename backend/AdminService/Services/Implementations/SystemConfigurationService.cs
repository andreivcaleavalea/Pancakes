using AdminService.Data;
using AdminService.Models.Entities;
using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AdminService.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AutoMapper;

namespace AdminService.Services.Implementations
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly AdminDbContext _context;
        private readonly ILogger<SystemConfigurationService> _logger;
        private readonly IMapper _mapper;

        public SystemConfigurationService(AdminDbContext context, ILogger<SystemConfigurationService> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
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

        public async Task<SystemConfiguration?> GetConfigurationAsync(Guid id)
        {
            return await _context.SystemConfigurations
                .FirstOrDefaultAsync(c => c.Id == id);
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

        public async Task<bool> DeleteConfigurationAsync(Guid id)
        {
            try
            {
                var config = await GetConfigurationAsync(id);
                if (config == null)
                    return false;

                _context.SystemConfigurations.Remove(config);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration with ID {Id}", id);
                return false;
            }
        }

        public async Task<PagedResponse<SystemConfigurationDto>> GetConfigurationsAsync(int page, int pageSize, string? category = null)
        {
            var query = _context.SystemConfigurations.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(c => c.Category == category);
            }

            var totalCount = await query.CountAsync();
            var configurations = await query
                .OrderBy(c => c.Category)
                .ThenBy(c => c.Key)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var configDtos = _mapper.Map<List<SystemConfigurationDto>>(configurations);

            return new PagedResponse<SystemConfigurationDto>
            {
                Data = configDtos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                HasPreviousPage = page > 1
            };
        }

        public async Task<SystemConfigurationDto> CreateConfigurationAsync(CreateSystemConfigurationRequest request, string updatedBy)
        {
            var configuration = new SystemConfiguration
            {
                Key = request.Key,
                Value = request.Value,
                Category = request.Category,
                Description = request.Description,
                DataType = request.DataType,
                IsSecret = request.IsSecret,
                UpdatedBy = updatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SystemConfigurations.Add(configuration);
            await _context.SaveChangesAsync();

            return _mapper.Map<SystemConfigurationDto>(configuration);
        }

        public async Task<SystemConfigurationDto> UpdateConfigurationAsync(Guid id, UpdateSystemConfigurationRequest request, string updatedBy)
        {
            var configuration = await GetConfigurationAsync(id);
            if (configuration == null)
                throw new ArgumentException($"Configuration with ID {id} not found");

            configuration.Value = request.Value;
            configuration.Category = request.Category;
            configuration.Description = request.Description;
            configuration.DataType = request.DataType;
            configuration.IsSecret = request.IsSecret;
            configuration.UpdatedBy = updatedBy;
            configuration.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<SystemConfigurationDto>(configuration);
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