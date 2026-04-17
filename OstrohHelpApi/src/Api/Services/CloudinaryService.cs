using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Application.Common.Interfaces.Services;
using Application.Common.Utilities;
using System.Text.Json;

namespace Api.Services;

public class CloudinaryService : IFileUploadService, IPreviewGenerationService
{
    private readonly Cloudinary _cloudinary;
    private const string ImageQualityDefault = "auto";

    public CloudinaryService(IConfiguration configuration)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "cloudinary-token.json");
        var json = File.ReadAllText(configPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        string apiKey = root.GetProperty("API_KEY").GetString()!;
        string apiSecret = root.GetProperty("API_SECRET").GetString()!;
        string cloudName = root.GetProperty("CLOUD_NAME").GetString()!;
        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    /// <summary>
    /// Helper method to build Cloudinary URLs with secure connection and transformation.
    /// Centralizes URL building logic to avoid duplication.
    /// </summary>
    private string BuildCloudinaryUrl(string publicId, Transformation transformation)
    {
        return new CloudinaryDotNet.Url(_cloudinary.Api.Account.Cloud)
            .Secure(true)
            .Transform(transformation)
            .BuildUrl(publicId);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string contentType)
    {
        if (contentType.StartsWith("image/"))
        {
            return await UploadImageAsync(fileStream, fileName, folder);
        }
        else if (contentType.StartsWith("video/"))
        {
            return await UploadVideoAsync(fileStream, fileName, folder);
        }
        else
        {
            return await UploadDocumentAsync(fileStream, fileName, folder);
        }
    }

    private async Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            PublicId = Path.GetFileNameWithoutExtension(fileName),
            Overwrite = true,
            Transformation = new Transformation().Quality(ImageQualityDefault)
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl?.ToString() ?? string.Empty;
    }

    private async Task<string> UploadVideoAsync(Stream fileStream, string fileName, string folder)
    {
        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            PublicId = Path.GetFileNameWithoutExtension(fileName),
            Overwrite = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl?.ToString() ?? string.Empty;
    }

    private async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string folder)
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            PublicId = Path.GetFileNameWithoutExtension(fileName),
            Overwrite = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl?.ToString() ?? string.Empty;
    }

    public string GetCompressedImageUrl(string publicId, int width = 0, int height = 0)
    {
        var transformation = new Transformation()
            .Quality(ImageQualityDefault)
            .FetchFormat("auto");

        if (width > 0 && height > 0)
        {
            transformation = transformation
                .Width(width)
                .Height(height)
                .Crop("fill");
        }
        else if (width > 0)
        {
            transformation = transformation.Width(width).Crop("scale");
        }
        else if (height > 0)
        {
            transformation = transformation.Height(height).Crop("scale");
        }

        return BuildCloudinaryUrl(publicId, transformation);
    }

    public string GetOptimizedVideoUrl(string publicId, int width = 0, int height = 0)
    {
        var transformation = new Transformation()
            .VideoCodec("h264")
            .AudioCodec("aac")
            .FetchFormat("mp4");

        if (width > 0 && height > 0)
        {
            transformation = transformation
                .Width(width)
                .Height(height)
                .Crop("fill");
        }

        return BuildCloudinaryUrl(publicId, transformation);
    }

    public async Task<string> DeleteFileAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        
        if (result.Result == "ok")
        {
            return "File deleted successfully";
        }
        
        return $"Failed to delete file: {result.Result}";
    }

    /// Generates a thumbnail preview URL for an image (150x150px).
    /// Uses Cloudinary transformations for on-the-fly generation.
    public string GenerateThumbnailUrl(string publicId)
    {
        var transformation = new Transformation()
            .Width(150)
            .Height(150)
            .Crop("fill")
            .Quality(40)
            .FetchFormat("auto");

        return BuildCloudinaryUrl(publicId, transformation);
    }

    /// Generates a medium-sized preview URL for an image (300x300px).
    public string GenerateMediumPreviewUrl(string publicId)
    {
        var transformation = new Transformation()
            .Width(300)
            .Height(300)
            .Crop("fill")
            .Quality(50)
            .FetchFormat("auto");

        return BuildCloudinaryUrl(publicId, transformation);
    }

    /// Generates a poster frame URL for a video (frame at 0:05 seconds, 300x169px).
    public string GenerateVideoPosterUrl(string publicId)
    {
        var transformation = new Transformation()
            .StartOffset("5") // Frame at 5 seconds
            .Width(300)
            .Height(169)
            .Crop("fill")
            .Quality(50)
            .FetchFormat("jpg");

        return BuildCloudinaryUrl(publicId, transformation);
    }

    /// Generates a first page preview URL for a PDF document (400x600px).
    /// Extracts page 1 from PDF.
    public string GeneratePdfPagePreviewUrl(string publicId)
    {
        var transformation = new Transformation()
            .Flags("page:1") // First page
            .Width(400)
            .Height(600)
            .Crop("fill")
            .Quality(30)
            .FetchFormat("jpg");

        return BuildCloudinaryUrl(publicId, transformation);
    }

    /// Gets the appropriate preview generation method based on file type.
    /// Returns null if preview is not applicable for the file type.
    public string? GeneratePreviewUrl(string publicId, string fileType)
    {
        var normalizedType = FileTypeNormalizer.Normalize(fileType);

        // Image types
        if (FileTypeNormalizer.IsImage(normalizedType))
        {
            return GenerateThumbnailUrl(publicId);
        }

        // Video types
        if (FileTypeNormalizer.IsVideo(normalizedType))
        {
            return GenerateVideoPosterUrl(publicId);
        }

        // PDF documents
        if (normalizedType is "pdf")
        {
            return GeneratePdfPagePreviewUrl(publicId);
        }

        // No preview available for other types (Word, Excel, TXT, etc.)
        return null;
    }
}
