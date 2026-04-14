using FluentValidation;

namespace Application.Messages.Validators;

public class FileUploadRequest
{
    public string FileName { get; set; }
    public string FileType { get; set; }
    public long FileSizeBytes { get; set; }
}

public class FileUploadValidator : AbstractValidator<FileUploadRequest>
{
    // File size limits
    private const long DocumentMaxSizeBytes = 100 * 1024 * 1024;  // 100 MB
    private const long VideoMaxSizeBytes = 500 * 1024 * 1024;     // 500 MB
    private const long ImageMaxSizeBytes = 50 * 1024 * 1024;      // 50 MB

    // Supported file types
    private static readonly string[] SupportedDocuments = { "pdf", "doc", "docx", "xlsx", "pptx", "txt", "zip" };
    private static readonly string[] SupportedImages = { "jpg", "jpeg", "png", "gif", "webp", "bmp" };
    private static readonly string[] SupportedVideos = { "mp4", "webm", "avi", "mov", "mkv", "flv", "m4v" };

    // Max attachments per message
    public const int MaxAttachmentsPerMessage = 6;

    public FileUploadValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name must not exceed 255 characters");

        RuleFor(x => x.FileType)
            .NotEmpty().WithMessage("File type is required")
            .Must(BeValidFileType).WithMessage("File type is not supported. Supported: pdf, doc, docx, xlsx, pptx, txt, zip, jpg, jpeg, png, gif, webp, bmp, mp4, webm, avi, mov, mkv, flv, m4v");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than 0 bytes")
            .Must((request, size) => ValidateFileSize(request.FileType, size))
            .WithMessage((request) => $"File exceeds maximum size for {GetFileCategory(request.FileType)} ({GetMaxSizeString(request.FileType)})");
    }

    private bool BeValidFileType(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType)) return false;

        var normalizedType = fileType.ToLowerInvariant().TrimStart('.');
        return SupportedDocuments.Contains(normalizedType) ||
               SupportedImages.Contains(normalizedType) ||
               SupportedVideos.Contains(normalizedType);
    }

    private bool ValidateFileSize(string fileType, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileType)) return false;

        var normalizedType = fileType.ToLowerInvariant().TrimStart('.');

        if (SupportedDocuments.Contains(normalizedType))
            return sizeBytes <= DocumentMaxSizeBytes;

        if (SupportedImages.Contains(normalizedType))
            return sizeBytes <= ImageMaxSizeBytes;

        if (SupportedVideos.Contains(normalizedType))
            return sizeBytes <= VideoMaxSizeBytes;

        return false;
    }

    private string GetFileCategory(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType)) return "file";

        var normalizedType = fileType.ToLowerInvariant().TrimStart('.');

        if (SupportedDocuments.Contains(normalizedType)) return "documents";
        if (SupportedImages.Contains(normalizedType)) return "images";
        if (SupportedVideos.Contains(normalizedType)) return "videos";

        return "file";
    }

    private string GetMaxSizeString(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType)) return "unknown";

        var normalizedType = fileType.ToLowerInvariant().TrimStart('.');

        if (SupportedDocuments.Contains(normalizedType)) return "100 MB";
        if (SupportedImages.Contains(normalizedType)) return "50 MB";
        if (SupportedVideos.Contains(normalizedType)) return "500 MB";

        return "unknown";
    }

    public static string GetAllowedFileTypes()
    {
        return "Documents: pdf, doc, docx, xlsx, pptx, txt, zip | " +
               "Images: jpg, jpeg, png, gif, webp, bmp | " +
               "Videos: mp4, webm, avi, mov, mkv, flv, m4v";
    }

    public static bool IsDocumentType(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType)) return false;
        return SupportedDocuments.Contains(fileType.ToLowerInvariant().TrimStart('.'));
    }

    public static bool IsImageType(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType)) return false;
        return SupportedImages.Contains(fileType.ToLowerInvariant().TrimStart('.'));
    }

    public static bool IsVideoType(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType)) return false;
        return SupportedVideos.Contains(fileType.ToLowerInvariant().TrimStart('.'));
    }
}
