namespace Application.Common.Interfaces.Services;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string contentType);
    Task<string> DeleteFileAsync(string publicId);
    string GetCompressedImageUrl(string publicId, int width = 0, int height = 0);
    string GetOptimizedVideoUrl(string publicId, int width = 0, int height = 0);
}
