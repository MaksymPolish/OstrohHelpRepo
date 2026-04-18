using Domain.AuditLogs;

namespace Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for audit log persistence
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Add a new audit log entry
    /// </summary>
    Task<AuditLog> AddAsync(AuditLog auditLog);

    /// <summary>
    /// Get audit log by ID
    /// </summary>
    Task<AuditLog?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all audit logs for a user (optionally filtered by date)
    /// </summary>
    Task<List<AuditLog>> GetByUserIdAsync(Guid userId, int? days = null);

    /// <summary>
    /// Get all audit logs for a resource
    /// </summary>
    Task<List<AuditLog>> GetByResourceAsync(string resource, Guid resourceId);

    /// <summary>
    /// Get all audit logs for an action type
    /// </summary>
    Task<List<AuditLog>> GetByActionAsync(string action, int? days = null);

    /// <summary>
    /// Get all audit logs (optionally paginated)
    /// </summary>
    Task<List<AuditLog>> GetAllAsync(int? skip = null, int? take = null);

    /// <summary>
    /// Delete audit log entries older than specified days
    /// </summary>
    Task<int> DeleteOlderThanAsync(int days);
}
