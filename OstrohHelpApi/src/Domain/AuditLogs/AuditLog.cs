namespace Domain.AuditLogs;

/// <summary>
/// Audit log entry for tracking user actions
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User who performed the action
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of action (e.g., "SendMessage", "DeleteMessage", "UploadAttachment")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Type of resource affected (e.g., "Message", "Attachment", "Consultation")
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// ID of the affected resource (nullable)
    /// </summary>
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// When the action was performed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the client who performed the action
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Status of the action ("Success", "Failed", etc.)
    /// </summary>
    public string Status { get; set; } = "Success";

    /// <summary>
    /// Additional JSON details about the action
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Error message if the action failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
