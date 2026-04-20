namespace Domain.AuditLogs;

/// Audit log entry for tracking user actions
public class AuditLog
{
    /// Unique identifier for the audit log entry
    public Guid Id { get; set; } = Guid.NewGuid();

    /// User who performed the action
    public Guid UserId { get; set; }

    /// Type of action (e.g., "SendMessage", "DeleteMessage", "UploadAttachment")
    public string Action { get; set; } = string.Empty;

    /// Type of resource affected (e.g., "Message", "Attachment", "Consultation")
    public string Resource { get; set; } = string.Empty;

    /// ID of the affected resource (nullable)
    public Guid? ResourceId { get; set; }

    /// When the action was performed
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// IP address of the client who performed the action
    public string? IpAddress { get; set; }

    /// Status of the action ("Success", "Failed", etc.)
    public string Status { get; set; } = "Success";

    /// Additional JSON details about the action
    public string? Details { get; set; }

    /// Error message if the action failed
    public string? ErrorMessage { get; set; }
}
