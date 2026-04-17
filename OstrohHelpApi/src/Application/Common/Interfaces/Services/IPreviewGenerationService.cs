namespace Application.Common.Interfaces.Services;

/// <summary>
/// Service for generating preview URLs using Cloudinary transformations.
/// This is a lightweight approach that generates transformed URLs on-the-fly
/// without storing physical files.
/// </summary>
public interface IPreviewGenerationService
{
    /// <summary>
    /// Generates a thumbnail preview URL for an image (150x150px).
    /// </summary>
    /// <param name="publicId">Cloudinary public ID of the image</param>
    /// <returns>Transformed URL for thumbnail preview</returns>
    string GenerateThumbnailUrl(string publicId);

    /// <summary>
    /// Generates a medium-sized preview URL for an image (300x300px).
    /// </summary>
    /// <param name="publicId">Cloudinary public ID of the image</param>
    /// <returns>Transformed URL for medium preview</returns>
    string GenerateMediumPreviewUrl(string publicId);

    /// <summary>
    /// Generates a poster frame URL for a video (frame at 0:05 seconds, 300x169px).
    /// </summary>
    /// <param name="publicId">Cloudinary public ID of the video</param>
    /// <returns>Transformed URL for video poster</returns>
    string GenerateVideoPosterUrl(string publicId);

    /// <summary>
    /// Generates a first page preview URL for a PDF document (400x600px).
    /// </summary>
    /// <param name="publicId">Cloudinary public ID of the PDF</param>
    /// <returns>Transformed URL for PDF page preview</returns>
    string GeneratePdfPagePreviewUrl(string publicId);

    /// <summary>
    /// Gets the appropriate preview generation method based on file type.
    /// </summary>
    /// <param name="publicId">Cloudinary public ID</param>
    /// <param name="fileType">File type (mime type or extension)</param>
    /// <returns>Transformed preview URL or null if preview not applicable</returns>
    string? GeneratePreviewUrl(string publicId, string fileType);
}
