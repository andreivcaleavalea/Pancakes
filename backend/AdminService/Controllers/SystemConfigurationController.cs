using AdminService.Models.DTOs;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public class SystemConfigurationController : ControllerBase
    {
        private readonly ISystemConfigurationService _systemConfigurationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<SystemConfigurationController> _logger;

        public SystemConfigurationController(
            ISystemConfigurationService systemConfigurationService,
            IAuditService auditService,
            ILogger<SystemConfigurationController> logger)
        {
            _systemConfigurationService = systemConfigurationService;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfigurations([FromQuery] string? category = null)
        {
            try
            {
                var configurations = await _systemConfigurationService.GetConfigurationsAsync(category);
                return Ok(new ApiResponse<List<SystemConfigurationDto>>
                {
                    Success = true,
                    Data = configurations,
                    Message = "System configurations retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system configurations");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving system configurations"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConfiguration(Guid id)
        {
            try
            {
                var configuration = await _systemConfigurationService.GetConfigurationAsync(id);
                if (configuration == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Configuration not found"
                    });
                }

                return Ok(new ApiResponse<SystemConfigurationDto>
                {
                    Success = true,
                    Data = configuration,
                    Message = "Configuration retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the configuration"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateConfiguration([FromBody] CreateSystemConfigurationRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var configuration = await _systemConfigurationService.CreateConfigurationAsync(request, currentAdminId);
                
                await _auditService.LogActionAsync(currentAdminId, "CONFIG_CREATED", "SystemConfiguration", 
                    configuration.Id.ToString(), request, GetClientIpAddress(), GetUserAgent());

                return CreatedAtAction(nameof(GetConfiguration), new { id = configuration.Id }, 
                    new ApiResponse<SystemConfigurationDto>
                    {
                        Success = true,
                        Data = configuration,
                        Message = "Configuration created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating configuration");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the configuration"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConfiguration(Guid id, [FromBody] UpdateSystemConfigurationRequest request)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var configuration = await _systemConfigurationService.UpdateConfigurationAsync(id, request, currentAdminId);
                
                if (configuration == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Configuration not found"
                    });
                }

                await _auditService.LogActionAsync(currentAdminId, "CONFIG_UPDATED", "SystemConfiguration", 
                    id.ToString(), request, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<SystemConfigurationDto>
                {
                    Success = true,
                    Data = configuration,
                    Message = "Configuration updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the configuration"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfiguration(Guid id)
        {
            try
            {
                var currentAdminId = GetCurrentAdminId();
                var success = await _systemConfigurationService.DeleteConfigurationAsync(id);
                
                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Configuration not found"
                    });
                }

                await _auditService.LogActionAsync(currentAdminId, "CONFIG_DELETED", "SystemConfiguration", 
                    id.ToString(), null, GetClientIpAddress(), GetUserAgent());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Configuration deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the configuration"
                });
            }
        }

        private string GetCurrentAdminId()
        {
            return HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }

        private string GetUserAgent()
        {
            return HttpContext.Request.Headers.UserAgent.ToString();
        }
    }
}