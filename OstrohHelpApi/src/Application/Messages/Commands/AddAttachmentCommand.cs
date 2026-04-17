using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Messages.Validators;
using Domain.Messages;
using MediatR;

namespace Application.Messages.Commands;

public class AddAttachmentCommand : IRequest<AddAttachmentResponse>
{
    public required Stream FileStream { get; set; }
    public required string FileName { get; set; }
    public required string FileType { get; set; }
    public required long FileSizeBytes { get; set; }
}

public class AddAttachmentResponse
{
    public required Guid AttachmentId { get; set; }
    public required string FileUrl { get; set; }
    public required string FileType { get; set; }
    public required long FileSizeBytes { get; set; }
    public required DateTime CreatedAt { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? MediumPreviewUrl { get; set; }
    public string? VideoPosterUrl { get; set; }
    public string? PdfPagePreviewUrl { get; set; }
}

public class AddAttachmentCommandHandler : IRequestHandler<AddAttachmentCommand, AddAttachmentResponse>
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IMessageAttachmentRepository _attachmentRepository;
    private readonly IPreviewGenerationService _previewGenerationService;
    private readonly FileUploadValidator _fileValidator;

    public AddAttachmentCommandHandler(
        IFileUploadService fileUploadService,
        IMessageAttachmentRepository attachmentRepository,
        IPreviewGenerationService previewGenerationService)
    {
        _fileUploadService = fileUploadService;
        _attachmentRepository = attachmentRepository;
        _previewGenerationService = previewGenerationService;
        _fileValidator = new FileUploadValidator();
    }

    public async Task<AddAttachmentResponse> Handle(AddAttachmentCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Validate file
        var validationRequest = new FileUploadRequest
        {
            FileName = request.FileName,
            FileType = request.FileType,
            FileSizeBytes = request.FileSizeBytes
        };

        var validationResult = await _fileValidator.ValidateAsync(validationRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException($"File validation failed: {errorMessage}");
        }

        // Step 2: Upload to Cloudinary
        request.FileStream.Position = 0;  // Reset stream position
        var fileUrl = await _fileUploadService.UploadFileAsync(
            request.FileStream,
            request.FileName,
            "attachments",
            GetContentType(request.FileType));

        if (string.IsNullOrEmpty(fileUrl))
        {
            throw new InvalidOperationException("Failed to upload file to Cloudinary");
        }

        // Step 3: Create attachment entity (standalone, no MessageId yet)
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(request.FileName);
        var cloudinaryPublicId = $"attachments/{fileNameWithoutExtension}";
        
        // Step 4: Generate preview URLs
        var thumbnailUrl = _previewGenerationService.GenerateThumbnailUrl(cloudinaryPublicId);
        var mediumPreviewUrl = _previewGenerationService.GenerateMediumPreviewUrl(cloudinaryPublicId);
        
        string? videoPosterUrl = null;
        string? pdfPagePreviewUrl = null;
        
        var normalizedFileType = request.FileType.ToLowerInvariant().TrimStart('.');
        if (normalizedFileType is "mp4" or "webm" or "mov" or "avi" or "mkv")
        {
            videoPosterUrl = _previewGenerationService.GenerateVideoPosterUrl(cloudinaryPublicId);
        }
        if (normalizedFileType is "pdf")
        {
            pdfPagePreviewUrl = _previewGenerationService.GeneratePdfPagePreviewUrl(cloudinaryPublicId);
        }
        
        var attachment = new MessageAttachment
        {
            Id = Guid.NewGuid(),
            MessageId = null,  // Standalone - not yet attached to a message
            FileUrl = fileUrl,
            FileType = request.FileType,
            FileSizeBytes = request.FileSizeBytes,
            CloudinaryPublicId = cloudinaryPublicId,
            CreatedAt = DateTime.UtcNow,
            ThumbnailUrl = thumbnailUrl,
            MediumPreviewUrl = mediumPreviewUrl,
            VideoPosterUrl = videoPosterUrl,
            PdfPagePreviewUrl = pdfPagePreviewUrl
        };

        // Step 5: Save to database
        await _attachmentRepository.AddAsync(attachment, cancellationToken);

        // Step 6: Return response
        return new AddAttachmentResponse
        {
            AttachmentId = attachment.Id,
            FileUrl = attachment.FileUrl,
            FileType = attachment.FileType,
            FileSizeBytes = attachment.FileSizeBytes,
            CreatedAt = attachment.CreatedAt,
            ThumbnailUrl = attachment.ThumbnailUrl,
            MediumPreviewUrl = attachment.MediumPreviewUrl,
            VideoPosterUrl = attachment.VideoPosterUrl,
            PdfPagePreviewUrl = attachment.PdfPagePreviewUrl
        };
    }

    private string GetContentType(string fileType)
    {
        var normalizedType = fileType.ToLowerInvariant().TrimStart('.');
        
        return normalizedType switch
        {
            // Images
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "webp" => "image/webp",
            "bmp" => "image/bmp",
            
            // Videos
            "mp4" => "video/mp4",
            "webm" => "video/webm",
            "avi" => "video/x-msvideo",
            "mov" => "video/quicktime",
            "mkv" => "video/x-matroska",
            "flv" => "video/x-flv",
            "m4v" => "video/x-m4v",
            
            // Documents
            "pdf" => "application/pdf",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "xls" => "application/vnd.ms-excel",
            "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "ppt" => "application/vnd.ms-powerpoint",
            "txt" => "text/plain",
            "zip" => "application/zip",
            
            _ => "application/octet-stream"
        };
    }
}
