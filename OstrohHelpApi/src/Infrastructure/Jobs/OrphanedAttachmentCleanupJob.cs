using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

/// <summary>
/// Background job that cleans up orphaned file attachments older than 7 days.
/// 
/// Orphaned attachments are files that were uploaded but never attached to any message.
/// They exist in the database with MessageId = null and should be removed to prevent
/// accumulation of unused files in Cloudinary and database storage.
/// 
/// Execution: Daily via Hangfire recurring job
/// </summary>
public class OrphanedAttachmentCleanupJob
{
    private readonly IMessageAttachmentRepository _attachmentRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<OrphanedAttachmentCleanupJob> _logger;
    
    // Configuration constants
    private const int OrphanedAttachmentAgeInDays = 7;

    public OrphanedAttachmentCleanupJob(
        IMessageAttachmentRepository attachmentRepository,
        IFileUploadService fileUploadService,
        ILogger<OrphanedAttachmentCleanupJob> logger)
    {
        _attachmentRepository = attachmentRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the cleanup of orphaned attachments.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Number of attachments cleaned up.</returns>
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting orphaned attachment cleanup job. Looking for attachments older than {Days} days",
                OrphanedAttachmentAgeInDays);

            // Calculate the cutoff date for orphaned attachments
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-OrphanedAttachmentAgeInDays);

            // Query all orphaned attachments (MessageId = null and CreatedAt < cutoff)
            var orphanedAttachments = await _attachmentRepository.GetOrphanedAttachmentsAsync(
                cutoffDate,
                cancellationToken);

            if (!orphanedAttachments.Any())
            {
                _logger.LogInformation("No orphaned attachments found to clean up");
                return 0;
            }

            _logger.LogInformation(
                "Found {Count} orphaned attachment(s) to clean up",
                orphanedAttachments.Count);

            int cleanedCount = 0;
            int failedCount = 0;

            foreach (var attachment in orphanedAttachments)
            {
                try
                {
                    // Delete file from Cloudinary using the stored public ID
                    await _fileUploadService.DeleteFileAsync(attachment.CloudinaryPublicId);
                    _logger.LogDebug(
                        "Deleted file from Cloudinary: {PublicId} (AttachmentId: {AttachmentId})",
                        attachment.CloudinaryPublicId,
                        attachment.Id);

                    // Delete attachment record from database
                    await _attachmentRepository.DeleteAsync(attachment, cancellationToken);
                    _logger.LogDebug(
                        "Deleted orphaned attachment from database: {AttachmentId}",
                        attachment.Id);

                    cleanedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(
                        ex,
                        "Failed to clean up orphaned attachment {AttachmentId}. Error: {ErrorMessage}",
                        attachment.Id,
                        ex.Message);
                }
            }

            _logger.LogInformation(
                "Orphaned attachment cleanup job completed. Cleaned: {CleanedCount}, Failed: {FailedCount}",
                cleanedCount,
                failedCount);

            return cleanedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Orphaned attachment cleanup job failed with error: {ErrorMessage}",
                ex.Message);
            throw;
        }
    }
}
