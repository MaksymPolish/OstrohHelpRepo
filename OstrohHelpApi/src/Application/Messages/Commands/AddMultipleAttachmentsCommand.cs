using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Utilities;
using Application.Messages.Validators;
using Domain.Messages;
using MediatR;

namespace Application.Messages.Commands;


/// Batch upload multiple files and create attachments
public class AddMultipleAttachmentsCommand : IRequest<AddMultipleAttachmentsResponse>
{
    public required List<BatchFileUpload> Files { get; set; }
    public Guid? MessageId { get; set; }  // Optional: if provided, attachments will be linked to message
}

public class BatchFileUpload
{
    public required Stream FileStream { get; set; }
    public required string FileName { get; set; }
    public required string FileType { get; set; }
    public required long FileSizeBytes { get; set; }
}

public class AddMultipleAttachmentsResponse
{
    public required List<BatchUploadResult> Results { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class BatchUploadResult
{
    public required Guid AttachmentId { get; set; }
    public required string FileName { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Response data (if successful)
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? MediumPreviewUrl { get; set; }
    public string? VideoPosterUrl { get; set; }
    public string? PdfPagePreviewUrl { get; set; }
}

public class AddMultipleAttachmentsCommandHandler : IRequestHandler<AddMultipleAttachmentsCommand, AddMultipleAttachmentsResponse>
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IMessageAttachmentRepository _attachmentRepository;
    private readonly IPreviewGenerationService _previewGenerationService;
    private readonly FileUploadValidator _fileValidator;

    public AddMultipleAttachmentsCommandHandler(
        IFileUploadService fileUploadService,
        IMessageAttachmentRepository attachmentRepository,
        IPreviewGenerationService previewGenerationService)
    {
        _fileUploadService = fileUploadService;
        _attachmentRepository = attachmentRepository;
        _previewGenerationService = previewGenerationService;
        _fileValidator = new FileUploadValidator();
    }

    public async Task<AddMultipleAttachmentsResponse> Handle(
        AddMultipleAttachmentsCommand request, 
        CancellationToken cancellationToken)
    {
        var results = new List<BatchUploadResult>();
        int successCount = 0;
        int failureCount = 0;

        foreach (var file in request.Files)
        {
            var result = new BatchUploadResult
            {
                AttachmentId = Guid.NewGuid(),
                FileName = file.FileName
            };

            try
            {
                // Step 1: Validate file
                var validationRequest = new FileUploadRequest
                {
                    FileName = file.FileName,
                    FileType = file.FileType,
                    FileSizeBytes = file.FileSizeBytes
                };

                var validationResult = await _fileValidator.ValidateAsync(validationRequest, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                    result.IsSuccess = false;
                    result.ErrorMessage = $"Validation failed: {errorMessage}";
                    failureCount++;
                    results.Add(result);
                    continue;
                }

                // Step 2: Upload to Cloudinary
                file.FileStream.Position = 0;
                var fileUrl = await _fileUploadService.UploadFileAsync(
                    file.FileStream,
                    file.FileName,
                    "attachments",
                    FileTypeNormalizer.GetMimeType(file.FileType));

                if (string.IsNullOrEmpty(fileUrl))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Failed to upload file to Cloudinary";
                    failureCount++;
                    results.Add(result);
                    continue;
                }

                // Step 3: Generate preview URLs
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                var cloudinaryPublicId = $"attachments/{fileNameWithoutExtension}";

                var thumbnailUrl = _previewGenerationService.GenerateThumbnailUrl(cloudinaryPublicId);
                var mediumPreviewUrl = _previewGenerationService.GenerateMediumPreviewUrl(cloudinaryPublicId);

                string? videoPosterUrl = null;
                string? pdfPagePreviewUrl = null;

                var normalizedFileType = FileTypeNormalizer.Normalize(file.FileType);
                if (FileTypeNormalizer.IsVideo(normalizedFileType))
                {
                    videoPosterUrl = _previewGenerationService.GenerateVideoPosterUrl(cloudinaryPublicId);
                }
                if (normalizedFileType is "pdf")
                {
                    pdfPagePreviewUrl = _previewGenerationService.GeneratePdfPagePreviewUrl(cloudinaryPublicId);
                }

                // Step 4: Create attachment entity
                var attachment = new MessageAttachment
                {
                    Id = result.AttachmentId,
                    MessageId = request.MessageId,  // Can be null if not attached yet
                    FileUrl = fileUrl,
                    FileType = file.FileType,
                    FileSizeBytes = file.FileSizeBytes,
                    CloudinaryPublicId = cloudinaryPublicId,
                    CreatedAt = DateTime.UtcNow,
                    ThumbnailUrl = thumbnailUrl,
                    MediumPreviewUrl = mediumPreviewUrl,
                    VideoPosterUrl = videoPosterUrl,
                    PdfPagePreviewUrl = pdfPagePreviewUrl
                };

                // Step 5: Save to database
                await _attachmentRepository.AddAsync(attachment, cancellationToken);

                // Step 6: Populate success result
                result.IsSuccess = true;
                result.FileUrl = attachment.FileUrl;
                result.FileType = attachment.FileType;
                result.FileSizeBytes = attachment.FileSizeBytes;
                result.CreatedAt = attachment.CreatedAt;
                result.ThumbnailUrl = attachment.ThumbnailUrl;
                result.MediumPreviewUrl = attachment.MediumPreviewUrl;
                result.VideoPosterUrl = attachment.VideoPosterUrl;
                result.PdfPagePreviewUrl = attachment.PdfPagePreviewUrl;
                successCount++;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Error: {ex.Message}";
                failureCount++;
            }

            results.Add(result);
        }

        return new AddMultipleAttachmentsResponse
        {
            Results = results,
            SuccessCount = successCount,
            FailureCount = failureCount,
            CompletedAt = DateTime.UtcNow
        };
    }
}
