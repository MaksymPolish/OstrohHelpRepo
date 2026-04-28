namespace Application.Messages.Dtos;

/// DTO for tracking batch upload progress in real-time (e.g., via SignalR)
public class BatchUploadProgressDto
{
    public Guid BatchId { get; set; }
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
    public int FailedCount { get; set; }
    public string? CurrentFileName { get; set; }
    public BatchUploadStatus Status { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<BatchUploadFileProgress> FileProgresses { get; set; } = new();
}

public class BatchUploadFileProgress
{
    public string FileName { get; set; } = string.Empty;
    public BatchUploadFileStatus Status { get; set; }
    public int ProgressPercentage { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? AttachmentId { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum BatchUploadStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    CompletedWithErrors = 3,
    Failed = 4,
    Cancelled = 5
}

public enum BatchUploadFileStatus
{
    Pending = 0,
    Uploading = 1,
    GeneratingPreviews = 2,
    Saving = 3,
    Completed = 4,
    Failed = 5,
    Skipped = 6
}
