using System;

namespace Api.Dtos;

public class MessageAttachmentDto
{
    public Guid Id { get; set; }
    public string FileUrl { get; set; }
    public string FileType { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }  // Soft delete flag - frontend decides how to display
    public string? ThumbnailUrl { get; set; }
    public string? MediumPreviewUrl { get; set; }
    public string? VideoPosterUrl { get; set; }
    public string? PdfPagePreviewUrl { get; set; }
}