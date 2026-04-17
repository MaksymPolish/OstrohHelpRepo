namespace Application.Common.Utilities;

/// <summary>
/// Utility for normalizing file types to lowercase without leading dots.
/// Centralizes file type normalization logic to avoid duplication across the codebase.
/// </summary>
public static class FileTypeNormalizer
{
    /// <summary>
    /// Normalizes a file type string to lowercase without leading dot.
    /// </summary>
    /// <param name="fileType">File type to normalize (e.g., ".jpg", "PDF", "mp4")</param>
    /// <returns>Normalized file type (e.g., "jpg", "pdf", "mp4")</returns>
    public static string Normalize(string? fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType))
            return string.Empty;

        return fileType.ToLowerInvariant().TrimStart('.');
    }

    /// <summary>
    /// Checks if a file type is an image.
    /// </summary>
    public static bool IsImage(string fileType)
    {
        var normalized = Normalize(fileType);
        return normalized is "jpg" or "jpeg" or "png" or "gif" or "webp" or "bmp";
    }

    /// <summary>
    /// Checks if a file type is a video.
    /// </summary>
    public static bool IsVideo(string fileType)
    {
        var normalized = Normalize(fileType);
        return normalized is "mp4" or "webm" or "mov" or "avi" or "mkv" or "flv" or "m4v";
    }

    /// <summary>
    /// Checks if a file type is a document.
    /// </summary>
    public static bool IsDocument(string fileType)
    {
        var normalized = Normalize(fileType);
        return normalized is "pdf" or "doc" or "docx" or "xlsx" or "xls" or "pptx" or "ppt" or "txt" or "zip";
    }

    /// <summary>
    /// Gets MIME type for a file type.
    /// </summary>
    public static string GetMimeType(string fileType)
    {
        var normalized = Normalize(fileType);

        return normalized switch
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
