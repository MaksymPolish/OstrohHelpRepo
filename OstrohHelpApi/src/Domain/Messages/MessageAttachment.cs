using System;

namespace Domain.Messages;

public class MessageAttachment
{
    public Guid Id { get; set; }
    
    // Nullable: NULL = standalone attachment (not yet attached to a message)
    // When user adds text to file, MessageId is set
    public Guid? MessageId { get; set; }
    
    public string FileUrl { get; set; }
    public string FileType { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Store Cloudinary public ID for deletion
    public string CloudinaryPublicId { get; set; }
    
    // Preview URLs - generated once and stored for reuse
    public string? ThumbnailUrl { get; set; }  // 150x150 thumbnail
    public string? MediumPreviewUrl { get; set; }  // 300x300 medium preview
    public string? VideoPosterUrl { get; set; }  // Video poster frame (5 seconds)
    public string? PdfPagePreviewUrl { get; set; }  // PDF first page preview
    
    // Soft delete flag - when true, attachment is hidden from users
    // FileUrl, FileType, and all preview URLs are cleared
    // Displayed as "Attachment was deleted by user" in API
    public bool IsDeleted { get; set; } = false;

    // Navigation property marked as NotMapped to prevent EF relationship configuration
    // due to type incompatibility (Message.Id is MessageId value object, this MessageId is Guid?)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Message? Message { get; set; }
}