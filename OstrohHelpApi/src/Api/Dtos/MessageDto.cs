using Domain.Messages;
using Domain.Users;

namespace Api.Dtos;

public class MessageDto
{
    public MessageId Id { get; set; }
    public string ConsultationId { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    
    // Message content - either encrypted or plaintext (for backward compatibility)
    public string? Text { get; set; }
    
    // Encrypted message fields (base64-encoded)
    // If these are set, Text should be ignored and decrypted client-side
    public string? EncryptedContent { get; set; }
    public string? Iv { get; set; }
    public string? AuthTag { get; set; }
    
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsDeleted { get; set; }  // Soft delete flag - frontend decides how to display
    public string FullNameSender { get; set; }
    public string FullNameReceiver { get; set; }
    public List<MessageAttachmentDto> Attachments { get; set; }
}