using Domain.AuditLogs;

namespace Application.Common.Services;

/// <summary>
/// Service for audit logging - tracks user actions
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Log a user action
    /// </summary>
    /// <param name="userId">User who performed the action</param>
    /// <param name="action">Action type (e.g., "SendMessage")</param>
    /// <param name="resource">Resource type (e.g., "Message")</param>
    /// <param name="resourceId">ID of the affected resource</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="details">Optional JSON details</param>
    /// <returns>Created audit log entry</returns>
    Task<AuditLog> LogAsync(Guid userId, string action, string resource, Guid? resourceId, string? ipAddress, string? details = null);

    /// <summary>
    /// Log a failed action
    /// </summary>
    Task<AuditLog> LogFailedAsync(Guid userId, string action, string resource, Guid? resourceId, string? ipAddress, string? errorMessage, string? details = null);

    /// <summary>
    /// Get all audit logs for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="days">Number of days to look back (optional)</param>
    /// <returns>List of audit log entries</returns>
    Task<List<AuditLog>> GetUserActionsAsync(Guid userId, int? days = null);

    /// <summary>
    /// Get audit log history for a specific resource
    /// </summary>
    /// <param name="resource">Resource type</param>
    /// <param name="resourceId">Resource identifier</param>
    /// <returns>List of audit log entries</returns>
    Task<List<AuditLog>> GetResourceHistoryAsync(string resource, Guid resourceId);

    /// <summary>
    /// Get all audit logs for a specific action type
    /// </summary>
    /// <param name="action">Action type (e.g., "SendMessage")</param>
    /// <param name="days">Number of days to look back (optional)</param>
    /// <returns>List of audit log entries</returns>
    Task<List<AuditLog>> GetActionHistoryAsync(string action, int? days = null);
}
