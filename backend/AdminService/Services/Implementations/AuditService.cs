using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Models.Entities;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AdminService.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly AdminDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AdminDbContext context, IMapper mapper, ILogger<AuditService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task LogActionAsync(string adminId, string action, string targetType, string targetId, object? details, string ipAddress, string userAgent)
        {
            try
            {
                var auditLog = new AdminAuditLog
                {
                    AdminId = adminId,
                    Action = action,
                    TargetType = targetType,
                    TargetId = targetId,
                    Details = details != null ? JsonSerializer.Serialize(details) : string.Empty,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                };

                _context.AdminAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action: {Action} by {AdminId}", action, adminId);
            }
        }

        public async Task<PagedResponse<AdminAuditLogDto>> GetAuditLogsAsync(int page, int pageSize, string? adminId = null, string? targetType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AdminAuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(adminId))
                query = query.Where(a => a.AdminId == adminId);

            if (!string.IsNullOrEmpty(targetType))
                query = query.Where(a => a.TargetType == targetType);

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value);

            var totalCount = await query.CountAsync();

            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var auditLogDtos = _mapper.Map<List<AdminAuditLogDto>>(auditLogs);

            return new PagedResponse<AdminAuditLogDto>
            {
                Data = auditLogDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNext = page * pageSize < totalCount,
                HasPrevious = page > 1
            };
        }

        public async Task<List<AdminAuditLogDto>> GetRecentActivityAsync(int count = 20)
        {
            var auditLogs = await _context.AdminAuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<List<AdminAuditLogDto>>(auditLogs);
        }

        public async Task<Dictionary<string, int>> GetActionStatsAsync(DateTime fromDate, DateTime toDate)
        {
            var stats = await _context.AdminAuditLogs
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Action, x => x.Count);

            return stats;
        }

        public async Task CleanupOldLogsAsync(int retentionDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            
            var oldLogs = _context.AdminAuditLogs.Where(a => a.Timestamp < cutoffDate);
            var count = await oldLogs.CountAsync();
            
            _context.AdminAuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} old audit logs older than {CutoffDate}", count, cutoffDate);
        }
    }
}