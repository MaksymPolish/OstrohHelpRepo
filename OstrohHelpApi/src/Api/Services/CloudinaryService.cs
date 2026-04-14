using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Application.Common.Interfaces.Services;
using System.Text.Json;

namespace Api.Services;

public class CloudinaryService : IFileUploadService
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

        var url = new CloudinaryDotNet.Url(_cloudinary.Api.Account.Cloud)
            .Transform(transformation)
            .BuildUrl(publicId);

        return new CloudinaryDotNet.Url(_cloudinary.Api.Account.Cloud)
            .Secure(true)
            .Transform(transformation)
            .BuildUrl(publicId);
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

        return new CloudinaryDotNet.Url(_cloudinary.Api.Account.Cloud)
            .Secure(true)
            .Transform(transformation)
            .BuildUrl(publicId);
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
}
