using Application.Common.Interfaces.Repositories;
using Domain.AuditLogs;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository for persisting audit log entries
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditLogRepository> _logger;

    public AuditLogRepository(ApplicationDbContext context, ILogger<AuditLogRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog)
    {
        try
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add audit log entry");
            throw;
        }
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit log by ID {Id}", id);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetByUserIdAsync(Guid userId, int? days = null)
    {
        try
        {
            var query = _context.AuditLogs.Where(a => a.UserId == userId);

            if (days.HasValue)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days.Value);
                query = query.Where(a => a.Timestamp >= cutoffDate);
            }

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetByResourceAsync(string resource, Guid resourceId)
    {
        try
        {
            return await _context.AuditLogs
                .Where(a => a.Resource == resource && a.ResourceId == resourceId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for resource {Resource}={ResourceId}", resource, resourceId);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetByActionAsync(string action, int? days = null)
    {
        try
        {
            var query = _context.AuditLogs.Where(a => a.Action == action);

            if (days.HasValue)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days.Value);
                query = query.Where(a => a.Timestamp >= cutoffDate);
            }

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for action {Action}", action);
            throw;
        }
    }

    public async Task<List<AuditLog>> GetAllAsync(int? skip = null, int? take = null)
    {
        try
        {
            IQueryable<AuditLog> query = _context.AuditLogs.OrderByDescending(a => a.Timestamp);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all audit logs");
            throw;
        }
    }

    public async Task<int> DeleteOlderThanAsync(int days)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var deleted = await _context.AuditLogs
                .Where(a => a.Timestamp < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Deleted {Count} audit log entries older than {Days} days", deleted, days);
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old audit logs");
            throw;
        }
    }
}
