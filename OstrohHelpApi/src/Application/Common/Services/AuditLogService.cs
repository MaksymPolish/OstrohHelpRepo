using Application.Common.Interfaces.Repositories;
using Domain.AuditLogs;
using Microsoft.Extensions.Logging;

namespace Application.Common.Services;

/// <summary>
/// Implementation of audit logging service
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IAuditLogRepository auditLogRepository, ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuditLog> LogAsync(Guid userId, string action, string resource, Guid? resourceId, string? ipAddress, string? details = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                Resource = resource,
                ResourceId = resourceId,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                Status = "Success",
                Details = details,
                ErrorMessage = null
            };

            var result = await _auditLogRepository.AddAsync(auditLog);

            _logger.LogInformation(
                "Audit log created: User={UserId}, Action={Action}, Resource={Resource}, ResourceId={ResourceId}",
                userId, action, resource, resourceId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for user {UserId}, action {Action}", userId, action);
            throw;
        }
    }

    public async Task<AuditLog> LogFailedAsync(Guid userId, string action, string resource, Guid? resourceId, string? ipAddress, string? errorMessage, string? details = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                Resource = resource,
                ResourceId = resourceId,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                Status = "Failed",
                Details = details,
                ErrorMessage = errorMessage
            };

            var result = await _auditLogRepository.AddAsync(auditLog);

            _logger.LogWarning(
                "Failed action logged: User={UserId}, Action={Action}, Resource={Resource}, Error={Error}",
                userId, action, resource, errorMessage);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log failed action for user {UserId}, action {Action}", userId, action);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetUserActionsAsync(Guid userId, int? days = null)
    {
        try
        {
            var result = await _auditLogRepository.GetByUserIdAsync(userId, days);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetResourceHistoryAsync(string resource, Guid resourceId)
    {
        try
        {
            var result = await _auditLogRepository.GetByResourceAsync(resource, resourceId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for resource {Resource}={ResourceId}", resource, resourceId);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetActionHistoryAsync(string action, int? days = null)
    {
        try
        {
            var result = await _auditLogRepository.GetByActionAsync(action, days);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for action {Action}", action);
            throw;
        }
    }
}
