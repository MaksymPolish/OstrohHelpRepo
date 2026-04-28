using Domain.Conferences;
using Domain.Users;

namespace Domain.Messages;

public class Message
{
    public Guid Id { get; set; }
    public Consultations Consultations { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    
    // Legacy: kept for backward compatibility
    public string? Text { get; set; }
    
    // Encryption fields (Phase 2)
    public byte[]? EncryptedContent { get; set; }  // AES-256-GCM ciphertext
    public byte[]? Iv { get; set; }                 // Initialization vector (12 bytes)
    public byte[]? AuthTag { get; set; }            // Authentication tag (16 bytes)
    
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Soft delete flag - when true, message content is hidden from users
    // Text/EncryptedContent is cleared, displayed as "Message was deleted by user"
    public bool IsDeleted { get; set; } = false;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<MessageAttachment> Attachments { get; set; } = new();

    /// Creates a message with encrypted content (preferred for new messages)
    public static Message CreateEncrypted(
        Guid id,
        Guid consultationId,
        Guid senderId,
        Guid receiverId,
        byte[] encryptedContent,
        byte[] iv,
        byte[] authTag,
        DateTime sentAt) =>
        new Message
        {
            Id = id,
            ConsultationId = consultationId,
            SenderId = senderId,
            ReceiverId = receiverId,
            EncryptedContent = encryptedContent,
            Iv = iv,
            AuthTag = authTag,
            IsRead = false,
            SentAt = sentAt,
            DeletedAt = null
        };

    /// Legacy: Creates a message with plaintext (for backward compatibility)
    public static Message Create(Guid id, Guid consultationId, Guid senderId, Guid receiverId, string text, bool isRead, DateTime sentAt, DateTime? deletedAt) =>
        new(id, consultationId, senderId, receiverId, text, isRead, sentAt, deletedAt);

    // Parameterless constructor for EF Core
    protected Message()
    {
    }

    private Message(Guid id, Guid consultationId, Guid senderId, Guid receiverId, string text, bool isRead, DateTime sentAt, DateTime? deletedAt)
    {
        Id = id;
        ConsultationId = consultationId;
        SenderId = senderId;
        ReceiverId = receiverId;
        Text = text;
        IsRead = isRead;
        SentAt = sentAt;
        DeletedAt = deletedAt;
    }

    public void UpdateText(string newText)
    {
        Text = newText;
    }

    public void UpdateEncryptedPayload(byte[] encryptedContent, byte[] iv, byte[] authTag)
    {
        EncryptedContent = encryptedContent;
        Iv = iv;
        AuthTag = authTag;
        Text = null; // Keep edited message in encrypted-only form.
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}