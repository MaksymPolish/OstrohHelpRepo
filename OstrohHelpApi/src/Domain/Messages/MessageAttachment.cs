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

    // Navigation property marked as NotMapped to prevent EF relationship configuration
    // due to type incompatibility (Message.Id is MessageId value object, this MessageId is Guid?)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Message? Message { get; set; }
}