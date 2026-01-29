using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace Api.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "cloudinary-token.json");
        var json = File.ReadAllText(configPath);
        dynamic secrets = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);
        string apiKey = secrets["API_KEY"];
        string apiSecret = secrets["API_SECRET"];
        string cloudName = secrets["CLOUD_NAME"];
        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string contentType)
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            PublicId = Path.GetFileNameWithoutExtension(fileName),
            Overwrite = true
        };

        if (contentType.StartsWith("image/"))
        {
            var imageParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                PublicId = Path.GetFileNameWithoutExtension(fileName),
                Overwrite = true
            };
            var result = await _cloudinary.UploadAsync(imageParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }
        else if (contentType.StartsWith("video/"))
        {
            var videoParams = new VideoUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                PublicId = Path.GetFileNameWithoutExtension(fileName),
                Overwrite = true
            };
            var result = await _cloudinary.UploadAsync(videoParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }
        else
        {
            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }
    }
}
